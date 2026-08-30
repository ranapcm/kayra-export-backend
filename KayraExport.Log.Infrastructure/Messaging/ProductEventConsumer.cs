using System.Text;
using System.Text.Json;
using KayraExport.Log.Application.Interfaces;
using KayraExport.Log.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KayraExport.Log.Infrastructure.Messaging;

public sealed class ProductEventConsumer : BackgroundService
{
    private const string ExchangeName = "kayra.events";
    private const string QueueName = "kayra.logs.product-events";

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductEventConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    public ProductEventConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<ProductEventConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connectionString =
            _configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException(
                "RabbitMQ connection string is missing.");

        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            DispatchConsumersAsync = true
        };

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: "product.#");

        _channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += HandleMessageAsync;

        // Manual acknowledgements prevent message loss before
        // the event has been persisted successfully.
        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Product event consumer started for queue {QueueName}",
            QueueName);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Product event consumer is stopping.");
        }
    }

    private async Task HandleMessageAsync(
        object sender,
        BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
        {
            return;
        }

        var payload = Encoding.UTF8.GetString(
            eventArgs.Body.ToArray());

        var eventType =
            eventArgs.BasicProperties.Type
            ?? eventArgs.RoutingKey;

        try
        {
            using var document = JsonDocument.Parse(payload);

            var occurredAt = document.RootElement.TryGetProperty(
                "OccurredAt",
                out var occurredAtElement)
                ? occurredAtElement.GetDateTime()
                : DateTime.UtcNow;

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<IEventLogRepository>();

            var logEntry = new EventLogEntry
            {
                ServiceName = "ProductService",
                EventType = eventType,
                RoutingKey = eventArgs.RoutingKey,
                Level = "Information",
                Payload = payload,
                OccurredAt = occurredAt,
                ReceivedAt = DateTime.UtcNow
            };

            await repository.AddAsync(logEntry);
            await repository.SaveChangesAsync();

            _channel.BasicAck(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false);

            _logger.LogInformation(
                "Event {EventType} stored with routing key {RoutingKey}",
                eventType,
                eventArgs.RoutingKey);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "RabbitMQ event {EventType} could not be processed.",
                eventType);

            await StoreErrorLogAsync(
                eventType,
                eventArgs.RoutingKey,
                payload,
                exception);

            // Invalid messages are rejected without requeueing to
            // prevent an endless poison-message processing loop.
            _channel.BasicNack(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false);
        }
    }

    private async Task StoreErrorLogAsync(
        string eventType,
        string routingKey,
        string originalPayload,
        Exception exception)
    {
        try
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<IEventLogRepository>();

            var errorPayload = JsonSerializer.Serialize(new
            {
                Message = exception.Message,
                ExceptionType = exception.GetType().FullName,
                OriginalPayload = originalPayload,
                FailedAt = DateTime.UtcNow
            });

            var errorLogEntry = new EventLogEntry
            {
                ServiceName = "LogService",
                EventType = eventType,
                RoutingKey = routingKey,
                Level = "Error",
                Payload = errorPayload,
                OccurredAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow
            };

            await repository.AddAsync(errorLogEntry);
            await repository.SaveChangesAsync();
        }
        catch (Exception loggingException)
        {
            // If centralized persistence is also unavailable,
            // the critical failure remains visible in stdout.
            _logger.LogCritical(
                loggingException,
                "The centralized error log could not be persisted.");
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

        base.Dispose();
    }
}
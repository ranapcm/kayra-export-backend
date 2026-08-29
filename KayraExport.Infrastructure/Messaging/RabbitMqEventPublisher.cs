using System.Text.Json;
using KayraExport.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace KayraExport.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher
    : IEventPublisher, IDisposable
{
    private const string ExchangeName = "kayra.events";

    private readonly IConnection _connection;

    public RabbitMqEventPublisher(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException(
                "RabbitMQ connection string is missing.");

        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _connection = connectionFactory.CreateConnection();
    }

    public Task PublishAsync<T>(
        T integrationEvent,
        string routingKey,
        CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);

        var body = JsonSerializer.SerializeToUtf8Bytes(
            integrationEvent);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = typeof(T).Name;
        properties.MessageId = Guid.NewGuid().ToString();

        channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
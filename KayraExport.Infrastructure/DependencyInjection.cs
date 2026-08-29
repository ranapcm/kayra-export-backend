using KayraExport.Application.Interfaces;
using KayraExport.Infrastructure.Messaging;
using KayraExport.Infrastructure.Persistence;
using KayraExport.Infrastructure.Repositories;
using KayraExport.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KayraExport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSql")));

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException(
                    "Redis connection string is missing.")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
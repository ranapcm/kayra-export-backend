using KayraExport.Log.Application.Interfaces;
using KayraExport.Log.Infrastructure.Messaging;
using KayraExport.Log.Infrastructure.Persistence;
using KayraExport.Log.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KayraExport.Log.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("LogPostgreSql")
            ?? throw new InvalidOperationException(
                "Log PostgreSQL connection string is missing.");

        services.AddDbContext<LogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEventLogRepository, EventLogRepository>();
        services.AddHostedService<ProductEventConsumer>();

        return services;
    }
}
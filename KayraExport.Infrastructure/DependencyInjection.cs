using KayraExport.Application.Interfaces;
using KayraExport.Infrastructure.Authentication;
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

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException(
                    "Redis connection string is missing.")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
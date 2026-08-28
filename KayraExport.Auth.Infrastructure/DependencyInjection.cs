using KayraExport.Auth.Application.Interfaces;
using KayraExport.Auth.Infrastructure.Authentication;
using KayraExport.Auth.Infrastructure.Identity;
using KayraExport.Auth.Infrastructure.Persistence;
using KayraExport.Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KayraExport.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("AuthPostgreSql")
            ?? throw new InvalidOperationException(
                "Auth PostgreSQL connection string is missing.");

        services.Configure<JwtSettings>(
            configuration.GetSection(
                JwtSettings.SectionName));

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AuthDbContext>();

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
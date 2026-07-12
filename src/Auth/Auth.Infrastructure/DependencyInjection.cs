using System.Security.Cryptography;
using Auth.Application;
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Database;
using Auth.Infrastructure.Roles.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddApplication()
            .AddDatabase(configuration)
            .AddAuthServices()
            .AddAuthenticationInternal(configuration)
            .AddMessaging(configuration);

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<AuthDbContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AuthDbContext>());

        return services;
    }

    private static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
#pragma warning disable CA2000
        var rsa = RSA.Create();
#pragma warning restore CA2000
        string publicKeyPath = configuration["Jwt:PublicKeyPath"]!;
        rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
        var securityKey = new RsaSecurityKey(rsa) { KeyId = "auth-key" };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = securityKey
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RoleCreatedConsumer>();
            x.AddConsumer<RoleUpdatedConsumer>();
            x.AddConsumer<RoleDeletedConsumer>();

            x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);

                o.UsePostgres();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:User"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapJwks();
        return app;
    }
}

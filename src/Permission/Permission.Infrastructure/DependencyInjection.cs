using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Permission.Application;
using Permission.Application.Abstractions.Data;
using Permission.Infrastructure.Database;

namespace Permission.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddApplication()
            .AddDatabase(configuration)
            .AddAuthenticationInternal(configuration)
            .AddMessaging();

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<PermissionDbContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IPermissionDbContext>(sp => sp.GetRequiredService<PermissionDbContext>());

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        using HttpClient client = new();
                        string jwksUrl = configuration["Jwt:JwksUrl"]!;
                        string jwksJson = client.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                        var jwks = new JsonWebKeySet(jwksJson);
                        return jwks.GetSigningKeys();
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<PermissionDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);

                o.UsePostgres();
            });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

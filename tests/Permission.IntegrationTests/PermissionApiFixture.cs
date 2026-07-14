using System.Security.Claims;
using System.Security.Cryptography;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Permission.Application.Abstractions.Data;
using Permission.Infrastructure.Database;
using TestShared;

namespace Permission.IntegrationTests;

public sealed class PermissionApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Database", PostgreSqlFixture.Instance.DefaultConnectionString);
        builder.UseSetting("Jwt:Issuer", "auth-service");
        builder.UseSetting("Jwt:Audience", "expense-tracker");
        builder.UseSetting("Serilog:Using", string.Empty);

        builder.ConfigureServices(services =>
        {
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "auth-service",
                    ValidAudience = "expense-tracker",
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = PermissionTestHelper.GetTestSecurityKey()
                };
            });

            RemoveMassTransitServices(services);

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        });
    }

    private static void RemoveMassTransitServices(IServiceCollection services)
    {
        var toRemove = services
            .Where(s =>
                (s.ServiceType.Namespace?.StartsWith("MassTransit", StringComparison.Ordinal) ?? false) ||
                (s.ImplementationType?.Namespace?.StartsWith("MassTransit", StringComparison.Ordinal) ?? false))
            .ToList();

        foreach (ServiceDescriptor descriptor in toRemove)
        {
            services.Remove(descriptor);
        }

        services.RemoveAll<IHealthCheck>();
    }

    public async Task InitializeDatabaseAsync()
    {
        await PostgreSqlFixture.Instance.InitializeAsync();

        using IServiceScope scope = Server.Services.CreateScope();
        PermissionDbContext context = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task CleanDatabaseAsync()
    {
        using IServiceScope scope = Server.Services.CreateScope();
        PermissionDbContext context = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE role_permissions, roles, outbox_message, outbox_state, inbox_state CASCADE");

        await SeedDataAsync(context);
    }

    private static async Task SeedDataAsync(PermissionDbContext context)
    {
        context.Roles.AddRange(
            new Permission.Domain.Roles.Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Admin"
            },
            new Permission.Domain.Roles.Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Viewer"
            },
            new Permission.Domain.Roles.Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Standard"
            });

        context.RolePermissions.AddRange(
            new Permission.Domain.Roles.RolePermission
            {
                RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Permission = "expenses:create"
            },
            new Permission.Domain.Roles.RolePermission
            {
                RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Permission = "expenses:read"
            },
            new Permission.Domain.Roles.RolePermission
            {
                RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Permission = "expenses:create"
            },
            new Permission.Domain.Roles.RolePermission
            {
                RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Permission = "expenses:read"
            });

        await context.SaveChangesAsync();
    }
}

public sealed class PermissionApiFixture : IAsyncLifetime
{
    public PermissionApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new PermissionApiFactory();
        await Factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}

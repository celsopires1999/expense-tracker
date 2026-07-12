extern alias AuthApi;

using System.Globalization;
using System.Security.Cryptography;
using Auth.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Permission.Application.Abstractions.Data;
using Permission.Infrastructure.Database;
using TestShared;

namespace Permission.IntegrationTests.RoleSync;

public sealed class RoleSyncFixture : IAsyncLifetime
{
    private const string PermDbName = "permission_role_sync_test";
    private const string AuthDbName = "auth_role_sync_test";

    private readonly Lazy<RSA> _rsaLazy = new(RSA.Create);

    public string PermissionConnectionString { get; private set; } = string.Empty;
    public string AuthConnectionString { get; private set; } = string.Empty;
    public RsaSecurityKey TestSecurityKey => new(_rsaLazy.Value) { KeyId = "test-key" };
    public string PrivateKeyPath { get; private set; } = string.Empty;
    public string PublicKeyPath { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await PostgreSqlFixture.Instance.InitializeAsync();

        await PostgreSqlFixture.Instance.CreateDatabaseAsync(PermDbName);
        await PostgreSqlFixture.Instance.CreateDatabaseAsync(AuthDbName);

        PermissionConnectionString = PostgreSqlFixture.Instance.GetConnectionString(PermDbName);
        AuthConnectionString = PostgreSqlFixture.Instance.GetConnectionString(AuthDbName);

        await RabbitMQFixture.Instance.InitializeAsync();

        PrivateKeyPath = Path.Combine(Path.GetTempPath(), $"rolesync-priv-{Guid.NewGuid():N}.pem");
        await File.WriteAllTextAsync(PrivateKeyPath, _rsaLazy.Value.ExportPkcs8PrivateKeyPem());

        PublicKeyPath = Path.Combine(Path.GetTempPath(), $"rolesync-pub-{Guid.NewGuid():N}.pem");
        await File.WriteAllTextAsync(PublicKeyPath, _rsaLazy.Value.ExportSubjectPublicKeyInfoPem());
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public sealed class RoleSyncPermissionApiFactory(RoleSyncFixture fixture) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Database", fixture.PermissionConnectionString);
        builder.UseSetting("RabbitMQ:Host", "host.docker.internal");
        builder.UseSetting("RabbitMQ:Port", RabbitMQFixture.Instance.Port.ToString(CultureInfo.InvariantCulture));
        builder.UseSetting("RabbitMQ:User", "guest");
        builder.UseSetting("RabbitMQ:Password", "guest");
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
                    IssuerSigningKey = fixture.TestSecurityKey
                };
            });
        });
    }

    public async Task InitializeDatabaseAsync()
    {
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
    }
}

public sealed class RoleSyncAuthApiFactory(RoleSyncFixture fixture) : WebApplicationFactory<AuthApi::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Database", fixture.AuthConnectionString);
        builder.UseSetting("RabbitMQ:Host", "host.docker.internal");
        builder.UseSetting("RabbitMQ:Port", RabbitMQFixture.Instance.Port.ToString(CultureInfo.InvariantCulture));
        builder.UseSetting("RabbitMQ:User", "guest");
        builder.UseSetting("RabbitMQ:Password", "guest");
        builder.UseSetting("Jwt:PrivateKeyPath", fixture.PrivateKeyPath);
        builder.UseSetting("Jwt:PublicKeyPath", fixture.PublicKeyPath);
        builder.UseSetting("Jwt:Issuer", "auth-service");
        builder.UseSetting("Jwt:Audience", "expense-tracker");
        builder.UseSetting("Jwt:ExpirationInMinutes", "60");
        builder.UseSetting("Serilog:Using", string.Empty);
    }

    public async Task InitializeDatabaseAsync()
    {
        using IServiceScope scope = Server.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task CleanDatabaseAsync()
    {
        using IServiceScope scope = Server.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE users, roles, user_roles, outbox_message, outbox_state, inbox_state CASCADE");
    }
}

using Auth.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestShared;

namespace Auth.IntegrationTests;

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Database", PostgreSqlFixture.Instance.DefaultConnectionString);
        builder.UseSetting("Jwt:PrivateKeyPath", PostgreSqlFixture.Instance.PrivateKey);
        builder.UseSetting("Jwt:PublicKeyPath", PostgreSqlFixture.Instance.PublicKey);
        builder.UseSetting("Jwt:Issuer", "auth-service");
        builder.UseSetting("Jwt:Audience", "expense-tracker");
        builder.UseSetting("Jwt:ExpirationInMinutes", "60");
        builder.UseSetting("Serilog:Using", string.Empty);
    }

    public async Task InitializeDatabaseAsync()
    {
        await PostgreSqlFixture.Instance.InitializeAsync();

        using IServiceScope scope = Server.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await context.Database.EnsureCreatedAsync();
        await SeedDataAsync(context);
    }

    public async Task CleanDatabaseAsync()
    {
        using IServiceScope scope = Server.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE users, roles, user_roles, outbox_message, outbox_state, inbox_state CASCADE");

        await SeedDataAsync(context);
    }

    private static async Task SeedDataAsync(AuthDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Auth.Domain.Roles.Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Admin"
                },
                new Auth.Domain.Roles.Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Viewer"
                },
                new Auth.Domain.Roles.Role
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Standard"
                });

            await context.SaveChangesAsync();
        }
    }
}

public sealed class AuthApiFixture : IAsyncLifetime
{
    public AuthApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new AuthApiFactory();
        await Factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}

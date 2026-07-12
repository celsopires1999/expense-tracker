using System.Security.Claims;
using System.Security.Cryptography;
using Expense.Application.Abstractions.Authentication;
using Expense.Application.Abstractions.Data;
using Expense.Infrastructure.Database;
using Expense.Infrastructure.DomainEvents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TestShared;

namespace Expense.IntegrationTests;

public sealed class ExpenseApiFactory : WebApplicationFactory<Program>
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
                    IssuerSigningKey = ExpenseTestHelper.GetTestSecurityKey()
                };
            });

            services.AddSingleton<IPermissionServiceClient, TestPermissionServiceClient>();
            services.AddSingleton<IUserContext>(new TestUserContext(ExpenseTestHelper.UserId));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        await PostgreSqlFixture.Instance.InitializeAsync();

        using IServiceScope scope = Server.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task CleanDatabaseAsync()
    {
        using IServiceScope scope = Server.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE expense_tags, expenses, tags, categories, payment_methods, outbox_message, outbox_state, inbox_state CASCADE");

        await SeedDataAsync(context);
    }

    private static async Task SeedDataAsync(ApplicationDbContext context)
    {
        context.Categories.AddRange(
            new Expense.Domain.Categories.Category { Id = ExpenseTestHelper.AlimentacaoCategoryId, Name = "Alimentacao" },
            new Expense.Domain.Categories.Category { Id = new Guid("44444444-4444-4444-4444-444444444444"), Name = "Transporte" });

        context.PaymentMethods.AddRange(
            new Expense.Domain.PaymentMethods.PaymentMethod { Id = ExpenseTestHelper.CreditoPaymentMethodId, Name = "Credito" },
            new Expense.Domain.PaymentMethods.PaymentMethod { Id = ExpenseTestHelper.PixPaymentMethodId, Name = "Pix" });

        await context.SaveChangesAsync();
    }
}

public sealed class ExpenseApiFixture : IAsyncLifetime
{
    public ExpenseApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new ExpenseApiFactory();
        await Factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}

internal sealed class TestUserContext(Guid userId) : IUserContext
{
    public Guid UserId => userId;
}

internal sealed class TestPermissionServiceClient : IPermissionServiceClient
{
    private static readonly HashSet<string> AllPermissions =
    [
        "expenses:create", "expenses:read", "expenses:read:all",
        "expenses:update", "expenses:update:all",
        "expenses:delete", "expenses:delete:all",
        "categories:create", "categories:read", "categories:update", "categories:delete",
        "payment-methods:create", "payment-methods:read", "payment-methods:update", "payment-methods:delete",
        "tags:create", "tags:read", "tags:update", "tags:delete",
    ];

    public Task<HashSet<string>> ResolvePermissionsAsync(string[] roles, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AllPermissions);
    }
}

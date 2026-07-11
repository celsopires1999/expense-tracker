using System.Security.Claims;
using System.Security.Cryptography;
using Expense.Application.Abstractions.Authentication;
using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Categories.Create;
using Expense.Application.PaymentMethods.Create;
using Expense.Application.Tags.Create;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using TestShared;

namespace Expense.IntegrationTests;

public static class ExpenseTestHelper
{
    private const string TestUserIdString = "11111111-1111-1111-1111-111111111111";
    public static readonly Guid UserId = Guid.Parse(TestUserIdString);

    public static readonly Guid AlimentacaoCategoryId = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid CreditoPaymentMethodId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid PixPaymentMethodId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");

    private static readonly Lazy<RSA> TestRsaLazy = new(() =>
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(PostgreSqlFixture.Instance.PrivateKey));
        return rsa;
    });

    public static RsaSecurityKey GetTestSecurityKey()
    {
        return new RsaSecurityKey(TestRsaLazy.Value) { KeyId = "test-key" };
    }

    public static string CreateJwtToken(Guid? userId = null, string[]? roles = null)
    {
        Guid id = userId ?? UserId;
        string[] effectiveRoles = roles ?? ["Standard"];

        var securityKey = new RsaSecurityKey(TestRsaLazy.Value) { KeyId = "test-key" };
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.Email, "test@example.com"),
        };

        claims.AddRange(effectiveRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = credentials,
            Issuer = "auth-service",
            Audience = "expense-tracker"
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    public static HttpClient CreateAuthenticatedClient(ExpenseApiFactory factory, Guid? userId = null)
    {
        HttpClient client = factory.CreateClient();
        string token = CreateJwtToken(userId);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<Guid> CreateCategoryAsync(ExpenseApiFactory factory, string name = "Test Category")
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ICommandHandler<CreateCategoryCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateCategoryCommand, Guid>>();

        var command = new CreateCategoryCommand(name);
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);
        return result.Value;
    }

    public static async Task<Guid> CreatePaymentMethodAsync(ExpenseApiFactory factory, string name = "Test Payment Method")
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ICommandHandler<CreatePaymentMethodCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreatePaymentMethodCommand, Guid>>();

        var command = new CreatePaymentMethodCommand(name);
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);
        return result.Value;
    }

    public static async Task<Guid> CreateTagAsync(ExpenseApiFactory factory, string name = "Test Tag")
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ICommandHandler<CreateTagCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateTagCommand, Guid>>();

        var command = new CreateTagCommand(name);
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);
        return result.Value;
    }
}

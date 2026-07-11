using System.Security.Claims;
using System.Security.Cryptography;
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.Register;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using TestShared;

namespace Auth.IntegrationTests;

public static class AuthTestHelper
{
    private static readonly Lazy<RSA> TestRsaLazy = new(() =>
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(PostgreSqlFixture.Instance.PrivateKey));
        return rsa;
    });

    public static async Task<Guid> CreateTestUserAsync(
        AuthApiFactory factory,
        string email = "test@example.com",
        string password = "Password123!")
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        RegisterUserCommand command = new(email, "John", "Doe", password);
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        return result.Value;
    }

    public static async Task<string> GetJwtTokenAsync(
        AuthApiFactory factory,
        string email = "test@example.com",
        string password = "Password123!")
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AuthDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        Auth.Domain.Users.User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new InvalidOperationException($"User with email {email} not found");

        string[] roles = ["Standard"];
        return CreateJwt(user.Id, email, roles);
    }

    public static HttpClient CreateAuthenticatedClient(
        AuthApiFactory factory,
        string token)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        AuthApiFactory factory,
        string email = "test@example.com",
        string password = "Password123!")
    {
        string token = await GetJwtTokenAsync(factory, email, password);
        return CreateAuthenticatedClient(factory, token);
    }

    private static string CreateJwt(Guid userId, string email, string[] roles)
    {
        var securityKey = new RsaSecurityKey(TestRsaLazy.Value);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
}

using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TestShared;

namespace Permission.IntegrationTests;

public static class PermissionTestHelper
{
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
        Guid id = userId ?? Guid.NewGuid();
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

    public static HttpClient CreateAuthenticatedClient(PermissionApiFactory factory)
    {
        HttpClient client = factory.CreateClient();
        string token = CreateJwtToken();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

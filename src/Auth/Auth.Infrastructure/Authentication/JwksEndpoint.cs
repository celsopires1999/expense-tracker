using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Authentication;

internal static class JwksEndpoint
{
    public static void MapJwks(this WebApplication app)
    {
        app.MapGet("/.well-known/jwks", (IConfiguration configuration) =>
        {
            using var rsa = RSA.Create();
            string publicKeyPath = configuration["Jwt:PublicKeyPath"]!;
            rsa.ImportFromPem(File.ReadAllText(publicKeyPath));

            RsaSecurityKey key = new(rsa) { KeyId = "auth-service-key" };

            var jwks = new
            {
                keys = new[]
                {
                    new
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = key.KeyId,
                        alg = "RS256",
                        n = Base64UrlEncoder.Encode(key.Rsa.ExportParameters(false).Modulus),
                        e = Base64UrlEncoder.Encode(key.Rsa.ExportParameters(false).Exponent)
                    }
                }
            };

            return Results.Json(jwks);
        });
    }
}

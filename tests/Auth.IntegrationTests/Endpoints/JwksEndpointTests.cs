using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Auth.IntegrationTests.Endpoints;

[Collection("Auth")]
public sealed class JwksEndpointTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public JwksEndpointTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Jwks_ShouldReturn200_WithValidJson()
    {
        HttpResponseMessage response = await _client.GetAsync("/.well-known/jwks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        JsonElement content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(content.TryGetProperty("keys", out JsonElement keys));
        Assert.Equal(JsonValueKind.Array, keys.ValueKind);
        Assert.True(keys.GetArrayLength() > 0);

        JsonElement key = keys[0];
        Assert.Equal("RSA", key.GetProperty("kty").GetString());
        Assert.Equal("sig", key.GetProperty("use").GetString());
        Assert.Equal("RS256", key.GetProperty("alg").GetString());
        Assert.Equal("auth-service-key", key.GetProperty("kid").GetString());
    }

    [Fact]
    public async Task Jwks_ShouldContainKey_ThatCanVerifyToken()
    {
        await AuthTestHelper.CreateTestUserAsync(_factory, "jwks@example.com", "Password123!");
        _ = await AuthTestHelper.GetJwtTokenAsync(_factory, "jwks@example.com", "Password123!");

        HttpResponseMessage jwksResponse = await _client.GetAsync("/.well-known/jwks");
        Assert.Equal(HttpStatusCode.OK, jwksResponse.StatusCode);

        JsonElement jwks = await jwksResponse.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement keys = jwks.GetProperty("keys");
        Assert.True(keys.GetArrayLength() > 0);
    }
}

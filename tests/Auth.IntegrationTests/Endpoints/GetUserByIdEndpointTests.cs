using System.Net;
using System.Net.Http.Json;

namespace Auth.IntegrationTests.Endpoints;

[Collection("Auth")]
public sealed class GetUserByIdEndpointTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public GetUserByIdEndpointTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
        await AuthTestHelper.CreateTestUserAsync(_factory, "auth@example.com", "Password123!");
        string token = await AuthTestHelper.GetJwtTokenAsync(_factory, "auth@example.com", "Password123!");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_ShouldReturn200_WithUser()
    {
        Guid userId = await AuthTestHelper.CreateTestUserAsync(_factory, "get@example.com", "Password123!");

        HttpResponseMessage response = await _client.GetAsync($"auth/users/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();
        var userId = Guid.NewGuid();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"auth/users/{userId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturn401_WithInvalidToken()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        HttpResponseMessage response = await _client.GetAsync($"auth/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_ForUnknownUser()
    {
        HttpResponseMessage response = await _client.GetAsync($"auth/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

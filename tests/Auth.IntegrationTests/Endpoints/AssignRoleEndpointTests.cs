using System.Net;

namespace Auth.IntegrationTests.Endpoints;

[Collection("Auth")]
public sealed class AssignRoleEndpointTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public AssignRoleEndpointTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
        await AuthTestHelper.CreateTestUserAsync(_factory, "role@example.com", "Password123!");
        string token = await AuthTestHelper.GetJwtTokenAsync(_factory, "role@example.com", "Password123!");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AssignRole_ShouldReturn200_WithValidToken()
    {
        Guid userId = await AuthTestHelper.CreateTestUserAsync(_factory, "assign@example.com", "Password123!");
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        HttpResponseMessage response = await _client.PostAsync($"auth/users/{userId}/roles/{roleId}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignRole_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        HttpResponseMessage response = await unauthenticatedClient.PostAsync($"auth/users/{userId}/roles/{roleId}", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

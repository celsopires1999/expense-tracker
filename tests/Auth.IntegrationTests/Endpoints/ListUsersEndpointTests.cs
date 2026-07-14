using System.Net;
using System.Net.Http.Json;
using Auth.Application.Users.GetAll;

namespace Auth.IntegrationTests.Endpoints;

[Collection("Auth")]
public sealed class ListUsersEndpointTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public ListUsersEndpointTests(AuthApiFixture fixture)
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
    public async Task ListUsers_ShouldReturn200_WithUsers()
    {
        HttpResponseMessage response = await _client.GetAsync("auth/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ListUsersResponse[]? users = await response.Content.ReadFromJsonAsync<ListUsersResponse[]>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task ListUsers_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync("auth/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListUsers_ShouldReturn401_WithInvalidToken()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        HttpResponseMessage response = await _client.GetAsync("auth/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListUsers_ShouldReturnEmptyArray_WhenNoUsersExist()
    {
        await _factory.CleanDatabaseAsync();

        HttpResponseMessage response = await _client.GetAsync("auth/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ListUsersResponse[]? users = await response.Content.ReadFromJsonAsync<ListUsersResponse[]>();
        Assert.NotNull(users);
        Assert.Empty(users);
    }
}

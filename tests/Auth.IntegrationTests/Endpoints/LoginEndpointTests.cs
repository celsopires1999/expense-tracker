using System.Net;
using System.Net.Http.Json;

namespace Auth.IntegrationTests.Endpoints;

[Collection("Auth")]
public sealed class LoginEndpointTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public LoginEndpointTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
        await AuthTestHelper.CreateTestUserAsync(_factory, "login@example.com", "Password123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_ShouldReturn200_WithToken()
    {
        object body = new
        {
            Email = "login@example.com",
            Password = "Password123!"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("auth/login", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LoginResponse? content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(content);
        Assert.False(string.IsNullOrWhiteSpace(content.Token));
    }

    [Fact]
    public async Task Login_ShouldReturn404_ForWrongCredentials()
    {
        object body = new
        {
            Email = "login@example.com",
            Password = "WrongPassword!"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("auth/login", body);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}

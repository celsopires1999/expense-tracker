using System.Net;
using System.Net.Http.Json;

namespace Auth.IntegrationTests.Endpoints;

[Collection("Auth")]
public sealed class RegisterEndpointTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;
    private readonly HttpClient _client;

    public RegisterEndpointTests(AuthApiFixture fixture)
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
    public async Task Register_ShouldReturn201_WithUserId()
    {
        object body = new
        {
            Email = "new@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123!"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("auth/register", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Guid userId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, userId);
    }

    [Fact]
    public async Task Register_ShouldReturn400_ForInvalidEmail()
    {
        object body = new
        {
            Email = "not-an-email",
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123!"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("auth/register", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturn400_ForShortPassword()
    {
        object body = new
        {
            Email = "short@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "123"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("auth/register", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturn409_ForDuplicateEmail()
    {
        object body = new
        {
            Email = "dup@example.com",
            FirstName = "First",
            LastName = "User",
            Password = "Password123!"
        };

        HttpResponseMessage first = await _client.PostAsJsonAsync("auth/register", body);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        HttpResponseMessage second = await _client.PostAsJsonAsync("auth/register", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}

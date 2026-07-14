using System.Net;
using System.Net.Http.Json;

namespace Expense.IntegrationTests.Endpoints;

[Collection("Expense")]
public sealed class TagsEndpointTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;
    private readonly HttpClient _client;

    public TagsEndpointTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
        _client = ExpenseTestHelper.CreateAuthenticatedClient(_factory);
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturn200_WithTags()
    {
        HttpResponseMessage response = await _client.GetAsync("tags");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<object>? tags = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(tags);
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync("tags");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn200_WithTagId()
    {
        object body = new { Name = "New Tag" };

        HttpResponseMessage response = await _client.PostAsJsonAsync("tags", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Guid? id = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenExists()
    {
        Guid tagId = await CreateTagAsync();

        object body = new { Name = "Updated Tag" };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"tags/{tagId}", body);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenExists()
    {
        Guid tagId = await CreateTagAsync();

        HttpResponseMessage response = await _client.DeleteAsync($"tags/{tagId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreateTagAsync()
    {
        object body = new { Name = $"Tag {Guid.NewGuid()}" };
        HttpResponseMessage response = await _client.PostAsJsonAsync("tags", body);
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }
}

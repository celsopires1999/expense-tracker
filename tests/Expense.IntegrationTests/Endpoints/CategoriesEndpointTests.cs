using System.Net;
using System.Net.Http.Json;

namespace Expense.IntegrationTests.Endpoints;

[Collection("Expense")]
public sealed class CategoriesEndpointTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;
    private readonly HttpClient _client;

    public CategoriesEndpointTests(ExpenseApiFixture fixture)
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
    public async Task GetAll_ShouldReturn200_WithCategories()
    {
        HttpResponseMessage response = await _client.GetAsync("categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<object>? categories = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(categories);
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync("categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn200_WithCategoryId()
    {
        object body = new { Name = "New Category" };

        HttpResponseMessage response = await _client.PostAsJsonAsync("categories", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Guid? id = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenExists()
    {
        Guid categoryId = await CreateCategoryAsync();

        object body = new { Name = "Updated Category" };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"categories/{categoryId}", body);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenExists()
    {
        Guid categoryId = await CreateCategoryAsync();

        HttpResponseMessage response = await _client.DeleteAsync($"categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreateCategoryAsync()
    {
        object body = new { Name = $"Category {Guid.NewGuid()}" };
        HttpResponseMessage response = await _client.PostAsJsonAsync("categories", body);
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }
}

using System.Net;
using System.Net.Http.Json;

namespace Expense.IntegrationTests.Endpoints;

[Collection("Expense")]
public sealed class ExpensesEndpointTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;
    private readonly HttpClient _client;

    public ExpensesEndpointTests(ExpenseApiFixture fixture)
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
    public async Task Create_ShouldReturn200_WithExpenseId()
    {
        object body = new
        {
            Description = "Lunch",
            Amount = 25.50m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
            TagIds = Array.Empty<Guid>()
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("expenses", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Guid? id = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Create_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        object body = new
        {
            Description = "Lunch",
            Amount = 25.50m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
            TagIds = Array.Empty<Guid>()
        };

        HttpResponseMessage response = await unauthenticatedClient.PostAsJsonAsync("expenses", body);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturn200_WithExpenses()
    {
        HttpResponseMessage response = await _client.GetAsync("expenses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<object>? expenses = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(expenses);
    }

    [Fact]
    public async Task Get_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync("expenses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenExists()
    {
        Guid expenseId = await CreateExpenseAsync();

        HttpResponseMessage response = await _client.GetAsync($"expenses/{expenseId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_ForUnknownId()
    {
        HttpResponseMessage response = await _client.GetAsync($"expenses/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenExists()
    {
        Guid expenseId = await CreateExpenseAsync();

        object body = new
        {
            Description = "Updated expense",
            Amount = 50.00m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
            TagIds = Array.Empty<Guid>()
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"expenses/{expenseId}", body);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenExists()
    {
        Guid expenseId = await CreateExpenseAsync();

        HttpResponseMessage response = await _client.DeleteAsync($"expenses/{expenseId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreateExpenseAsync()
    {
        object body = new
        {
            Description = "Test expense",
            Amount = 10.00m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
            TagIds = Array.Empty<Guid>()
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("expenses", body);
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }
}

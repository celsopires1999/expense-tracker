using System.Net;
using System.Net.Http.Json;

namespace Expense.IntegrationTests.Endpoints;

[Collection("Expense")]
public sealed class PaymentMethodsEndpointTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;
    private readonly HttpClient _client;

    public PaymentMethodsEndpointTests(ExpenseApiFixture fixture)
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
    public async Task GetAll_ShouldReturn200_WithPaymentMethods()
    {
        HttpResponseMessage response = await _client.GetAsync("payment-methods");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<object>? methods = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(methods);
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync("payment-methods");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn200_WithPaymentMethodId()
    {
        object body = new { Name = "New Payment Method" };

        HttpResponseMessage response = await _client.PostAsJsonAsync("payment-methods", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Guid? id = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenExists()
    {
        Guid methodId = await CreatePaymentMethodAsync();

        object body = new { Name = "Updated Method" };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"payment-methods/{methodId}", body);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenExists()
    {
        Guid methodId = await CreatePaymentMethodAsync();

        HttpResponseMessage response = await _client.DeleteAsync($"payment-methods/{methodId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreatePaymentMethodAsync()
    {
        object body = new { Name = $"Method {Guid.NewGuid()}" };
        HttpResponseMessage response = await _client.PostAsJsonAsync("payment-methods", body);
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }
}

using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Expenses.Get;
using Expense.Domain.Expenses;
using Expense.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using TestShared;

namespace Expense.IntegrationTests.Handlers;

[Collection("Expense")]
public sealed class GetExpensesQueryTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public GetExpensesQueryTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetExpenses_ShouldReturnAll_WhenNoFilters()
    {
        await SeedExpensesAsync(2);

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpensesQuery, List<ExpenseResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpensesQuery, List<ExpenseResponse>>>();

        var query = new GetExpensesQuery();
        Result<List<ExpenseResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetExpenses_ShouldFilterByUserId()
    {
        var otherUserId = Guid.NewGuid();
        await SeedExpensesAsync(2, ExpenseTestHelper.UserId);
        await SeedExpensesAsync(1, otherUserId);

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpensesQuery, List<ExpenseResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpensesQuery, List<ExpenseResponse>>>();

        var query = new GetExpensesQuery { UserId = ExpenseTestHelper.UserId };
        Result<List<ExpenseResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, e => Assert.Equal(ExpenseTestHelper.UserId, e.UserId));
    }

    [Fact]
    public async Task GetExpenses_ShouldFilterByCategoryId()
    {
        Guid categoryId = ExpenseTestHelper.AlimentacaoCategoryId;
        await SeedExpensesAsync(2, categoryId: categoryId);

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpensesQuery, List<ExpenseResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpensesQuery, List<ExpenseResponse>>>();

        var query = new GetExpensesQuery { CategoryId = categoryId };
        Result<List<ExpenseResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, e => Assert.Equal(categoryId, e.CategoryId));
    }

    [Fact]
    public async Task GetExpenses_ShouldReturnEmpty_WhenNoExpenses()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpensesQuery, List<ExpenseResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpensesQuery, List<ExpenseResponse>>>();

        var query = new GetExpensesQuery();
        Result<List<ExpenseResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetExpenses_ShouldFilterByStatus()
    {
        await SeedExpensesAsync(2, status: ExpenseStatus.Pending);
        await SeedExpensesAsync(1, status: ExpenseStatus.Approved);

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpensesQuery, List<ExpenseResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpensesQuery, List<ExpenseResponse>>>();

        var query = new GetExpensesQuery { Status = ExpenseStatus.Pending };
        Result<List<ExpenseResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, e => Assert.Equal(ExpenseStatus.Pending, e.Status));
    }

    private async Task SeedExpensesAsync(int count, Guid? userId = null, Guid? categoryId = null, ExpenseStatus? status = null)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        for (int i = 0; i < count; i++)
        {
            var expense = new global::Expense.Domain.Expenses.Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? ExpenseTestHelper.UserId,
                Description = $"Expense {i}",
                Amount = 10 * (i + 1),
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)),
                CategoryId = categoryId ?? ExpenseTestHelper.AlimentacaoCategoryId,
                PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
                Status = status ?? ExpenseStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            context.Expenses.Add(expense);
        }

        await context.SaveChangesAsync();
    }
}

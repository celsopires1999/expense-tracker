using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Expenses.GetById;
using Expense.Domain.Expenses;
using Expense.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using TestShared;

namespace Expense.IntegrationTests.Handlers;

[Collection("Expense")]
public sealed class GetExpenseByIdQueryTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public GetExpenseByIdQueryTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_ShouldReturnExpense_WhenExists()
    {
        Guid expenseId = await SeedExpenseAsync();

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpenseByIdQuery, ExpenseResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpenseByIdQuery, ExpenseResponse>>();

        var query = new GetExpenseByIdQuery(expenseId);
        Result<ExpenseResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expenseId, result.Value.Id);
        Assert.Equal("Test expense", result.Value.Description);
        Assert.Equal(42.50m, result.Value.Amount);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_ForUnknownId()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetExpenseByIdQuery, ExpenseResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetExpenseByIdQuery, ExpenseResponse>>();

        var query = new GetExpenseByIdQuery(Guid.NewGuid());
        Result<ExpenseResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Expenses.NotFound", result.Error.Code);
    }

    private async Task<Guid> SeedExpenseAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Id = Guid.NewGuid(),
            UserId = ExpenseTestHelper.UserId,
            Description = "Test expense",
            Amount = 42.50m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
            CreatedAt = DateTime.UtcNow
        };

        context.Expenses.Add(expense);
        await context.SaveChangesAsync();
        return expense.Id;
    }
}

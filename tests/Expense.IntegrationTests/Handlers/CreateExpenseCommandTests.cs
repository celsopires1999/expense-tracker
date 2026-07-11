using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Expenses.Create;
using Expense.Domain.Expenses;
using Expense.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using TestShared;

namespace Expense.IntegrationTests.Handlers;

[Collection("Expense")]
public sealed class CreateExpenseCommandTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public CreateExpenseCommandTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldCreateExpense_InDatabase()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateExpenseCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateExpenseCommand, Guid>>();

        var command = new CreateExpenseCommand
        {
            UserId = ExpenseTestHelper.UserId,
            Description = "Grocery shopping",
            Amount = 150.50m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId
        };

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        ApplicationDbContext context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        global::Expense.Domain.Expenses.Expense? expense = await context.Expenses.FindAsync(result.Value);

        Assert.NotNull(expense);
        Assert.Equal("Grocery shopping", expense.Description);
        Assert.Equal(150.50m, expense.Amount);
    }

    [Fact]
    public async Task Create_ShouldReturnFailure_WhenUserIdDoesNotMatch()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateExpenseCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateExpenseCommand, Guid>>();

        var command = new CreateExpenseCommand
        {
            UserId = Guid.NewGuid(),
            Description = "Test",
            Amount = 10,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId
        };

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Expenses.Unauthorized", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldReturnFailure_WhenCategoryNotFound()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateExpenseCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateExpenseCommand, Guid>>();

        var command = new CreateExpenseCommand
        {
            UserId = ExpenseTestHelper.UserId,
            Description = "Test",
            Amount = 10,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = Guid.NewGuid(),
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId
        };

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Categories.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldReturnFailure_WhenPaymentMethodNotFound()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateExpenseCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateExpenseCommand, Guid>>();

        var command = new CreateExpenseCommand
        {
            UserId = ExpenseTestHelper.UserId,
            Description = "Test",
            Amount = 10,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = Guid.NewGuid()
        };

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PaymentMethods.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldAssignTags_WhenTagIdsProvided()
    {
        Guid tagId = await ExpenseTestHelper.CreateTagAsync(_factory, "Groceries");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateExpenseCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateExpenseCommand, Guid>>();

        var command = new CreateExpenseCommand
        {
            UserId = ExpenseTestHelper.UserId,
            Description = "Lunch with tag",
            Amount = 25.00m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = ExpenseTestHelper.AlimentacaoCategoryId,
            PaymentMethodId = ExpenseTestHelper.CreditoPaymentMethodId,
            TagIds = [tagId]
        };

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        ApplicationDbContext context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        global::Expense.Domain.Expenses.Expense? expense = await context.Expenses
            .Include(e => e.Tags)
            .FirstAsync(e => e.Id == result.Value);

        Assert.Single(expense.Tags);
        Assert.Equal(tagId, expense.Tags[0].TagId);
    }
}

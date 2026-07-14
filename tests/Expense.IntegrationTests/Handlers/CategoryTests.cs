using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Categories.Create;
using Expense.Application.Categories.Delete;
using Expense.Application.Categories.GetAll;
using Expense.Application.Categories.Update;
using Expense.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using TestShared;

namespace Expense.IntegrationTests.Handlers;

[Collection("Expense")]
public sealed class CreateCategoryCommandTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public CreateCategoryCommandTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldCreateCategory_InDatabase()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateCategoryCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateCategoryCommand, Guid>>();

        var command = new CreateCategoryCommand("New Category");
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        ApplicationDbContext context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Expense.Domain.Categories.Category? category = await context.Categories.FindAsync(result.Value);

        Assert.NotNull(category);
        Assert.Equal("New Category", category.Name);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameExists()
    {
        await ExpenseTestHelper.CreateCategoryAsync(_factory, "Duplicate");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateCategoryCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateCategoryCommand, Guid>>();

        var command = new CreateCategoryCommand("Duplicate");
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Categories.DuplicateName", result.Error.Code);
    }
}

[Collection("Expense")]
public sealed class GetAllCategoriesQueryTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public GetAllCategoriesQueryTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturnSeededCategories()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>>>();

        var query = new GetAllCategoriesQuery();
        Result<List<CategoryResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrderedByName()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>>>();

        var query = new GetAllCategoriesQuery();
        Result<List<CategoryResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var names = result.Value.Select(c => c.Name).ToList();
        Assert.Equal(names, names.OrderBy(n => n));
    }
}

[Collection("Expense")]
public sealed class UpdateCategoryCommandTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public UpdateCategoryCommandTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Update_ShouldUpdateCategoryName()
    {
        Guid categoryId = await ExpenseTestHelper.CreateCategoryAsync(_factory, "Original");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<UpdateCategoryCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateCategoryCommand>>();

        var command = new UpdateCategoryCommand(categoryId, "Updated");
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        ApplicationDbContext context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Expense.Domain.Categories.Category? category = await context.Categories.FindAsync(categoryId);

        Assert.NotNull(category);
        Assert.Equal("Updated", category.Name);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_ForUnknownId()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<UpdateCategoryCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateCategoryCommand>>();

        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Updated");
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Categories.NotFound", result.Error.Code);
    }
}

[Collection("Expense")]
public sealed class DeleteCategoryCommandTests : IClassFixture<ExpenseApiFixture>, IAsyncLifetime
{
    private readonly ExpenseApiFactory _factory;

    public DeleteCategoryCommandTests(ExpenseApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Delete_ShouldRemoveCategory_FromDatabase()
    {
        Guid categoryId = await ExpenseTestHelper.CreateCategoryAsync(_factory, "ToDelete");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<DeleteCategoryCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteCategoryCommand>>();

        var command = new DeleteCategoryCommand(categoryId);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        ApplicationDbContext context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Expense.Domain.Categories.Category? category = await context.Categories.FindAsync(categoryId);

        Assert.Null(category);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_ForUnknownId()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<DeleteCategoryCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteCategoryCommand>>();

        var command = new DeleteCategoryCommand(Guid.NewGuid());
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Categories.NotFound", result.Error.Code);
    }
}

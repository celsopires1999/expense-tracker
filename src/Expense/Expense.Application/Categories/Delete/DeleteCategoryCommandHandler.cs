using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound(
                "Categories.NotFound",
                $"The category with Id = '{command.Id}' was not found"));
        }

        bool inUse = await context.Expenses
            .AnyAsync(e => e.CategoryId == command.Id, cancellationToken);

        if (inUse)
        {
            return Result.Failure(Error.Conflict(
                "Categories.InUse",
                "The category is currently in use by one or more expenses and cannot be deleted"));
        }

        context.Categories.Remove(category);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

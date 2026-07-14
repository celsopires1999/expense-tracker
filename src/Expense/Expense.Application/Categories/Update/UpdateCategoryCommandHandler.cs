using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound(
                "Categories.NotFound",
                $"The category with Id = '{command.Id}' was not found"));
        }

        bool duplicate = await context.Categories
            .AnyAsync(c => c.Name == command.Name && c.Id != command.Id, cancellationToken);

        if (duplicate)
        {
            return Result.Failure(Error.Conflict(
                "Categories.DuplicateName",
                $"A category with name '{command.Name}' already exists"));
        }

        category.Name = command.Name;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Categories.Create;

internal sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        bool exists = await context.Categories
            .AnyAsync(c => c.Name == command.Name, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Categories.DuplicateName",
                $"A category with name '{command.Name}' already exists"));
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name
        };

        context.Categories.Add(category);

        await context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

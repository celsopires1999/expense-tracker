using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Tags.Update;

internal sealed class UpdateTagCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateTagCommand>
{
    public async Task<Result> Handle(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        Tag? tag = await context.Tags
            .SingleOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (tag is null)
        {
            return Result.Failure(Error.NotFound(
                "Tags.NotFound",
                $"The tag with Id = '{command.Id}' was not found"));
        }

        bool duplicate = await context.Tags
            .AnyAsync(t => t.Name == command.Name && t.Id != command.Id, cancellationToken);

        if (duplicate)
        {
            return Result.Failure(Error.Conflict(
                "Tags.DuplicateName",
                $"A tag with name '{command.Name}' already exists"));
        }

        tag.Name = command.Name;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

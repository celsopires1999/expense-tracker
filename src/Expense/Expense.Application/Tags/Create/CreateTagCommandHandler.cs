using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Tags.Create;

internal sealed class CreateTagCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTagCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        bool exists = await context.Tags
            .AnyAsync(t => t.Name == command.Name, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Tags.DuplicateName",
                $"A tag with name '{command.Name}' already exists"));
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = command.Name
        };

        context.Tags.Add(tag);

        await context.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }
}

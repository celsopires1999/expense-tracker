using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tags;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tags.Delete;

internal sealed class DeleteTagCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteTagCommand>
{
    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        Tag? tag = await context.Tags
            .SingleOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (tag is null)
        {
            return Result.Failure(Error.NotFound(
                "Tags.NotFound",
                $"The tag with Id = '{command.Id}' was not found"));
        }

        bool inUse = await context.ExpenseTags
            .AnyAsync(et => et.TagId == command.Id, cancellationToken);

        if (inUse)
        {
            return Result.Failure(Error.Conflict(
                "Tags.InUse",
                "The tag is currently in use by one or more expenses and cannot be deleted"));
        }

        context.Tags.Remove(tag);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

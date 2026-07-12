using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using Permission.Domain.Roles;
using SharedKernel;

namespace Permission.Application.Roles.Update;

internal sealed class UpdateRoleCommandHandler(IPermissionDbContext context)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles
            .SingleOrDefaultAsync(r => r.Id == command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id '{command.RoleId}' was not found"));
        }

        role.Name = command.Name;

        role.Raise(new RoleUpdatedDomainEvent(role.Id));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

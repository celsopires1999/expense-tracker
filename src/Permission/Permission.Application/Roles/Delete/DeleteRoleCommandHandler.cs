using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using Permission.Domain.Roles;
using SharedKernel;

namespace Permission.Application.Roles.Delete;

internal sealed class DeleteRoleCommandHandler(IPermissionDbContext context)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles
            .SingleOrDefaultAsync(r => r.Id == command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id '{command.RoleId}' was not found"));
        }

        role.Raise(new RoleDeletedDomainEvent(role.Id));

        List<RolePermission> permissions = await context.RolePermissions
            .Where(rp => rp.RoleId == command.RoleId)
            .ToListAsync(cancellationToken);

        context.RolePermissions.RemoveRange(permissions);
        context.Roles.Remove(role);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

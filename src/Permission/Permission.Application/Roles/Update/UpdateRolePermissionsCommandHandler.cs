using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using Permission.Domain.Roles;
using SharedKernel;

namespace Permission.Application.Roles.Update;

internal sealed class UpdateRolePermissionsCommandHandler(IPermissionDbContext context)
    : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task<Result> Handle(UpdateRolePermissionsCommand command, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles
            .SingleOrDefaultAsync(r => r.Id == command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id '{command.RoleId}' was not found"));
        }

        role.Raise(new RoleUpdatedDomainEvent(role.Id));

        List<RolePermission> existing = await context.RolePermissions
            .Where(rp => rp.RoleId == command.RoleId)
            .ToListAsync(cancellationToken);

        context.RolePermissions.RemoveRange(existing);

        foreach (string permission in command.Permissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = command.RoleId,
                Permission = permission
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

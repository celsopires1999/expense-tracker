using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using Permission.Domain.Roles;
using SharedKernel;

namespace Permission.Application.Roles.Create;

internal sealed class CreateRoleCommandHandler(IPermissionDbContext context)
    : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = command.Name
        };

        role.Raise(new RoleCreatedDomainEvent(role.Id));

        context.Roles.Add(role);

        foreach (string permission in command.Permissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}

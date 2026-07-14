using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Auth.Application.Users.RemoveRole;

internal sealed class RemoveRoleCommandHandler(IAuthDbContext context)
    : ICommandHandler<RemoveRoleCommand>
{
    public async Task<Result> Handle(RemoveRoleCommand command, CancellationToken cancellationToken)
    {
        Domain.Roles.UserRole? userRole = await context.UserRoles
            .SingleOrDefaultAsync(
                ur => ur.UserId == command.UserId && ur.RoleId == command.RoleId,
                cancellationToken);

        if (userRole is null)
        {
            return Result.Success();
        }

        context.UserRoles.Remove(userRole);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

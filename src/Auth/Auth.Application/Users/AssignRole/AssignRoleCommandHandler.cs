using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Roles;
using Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Auth.Application.Users.AssignRole;

internal sealed class AssignRoleCommandHandler(IAuthDbContext context)
    : ICommandHandler<AssignRoleCommand>
{
    public async Task<Result> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        if (!await context.Users.AnyAsync(u => u.Id == command.UserId, cancellationToken))
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (!await context.Roles.AnyAsync(r => r.Id == command.RoleId, cancellationToken))
        {
            return Result.Failure(UserErrors.RoleNotFound);
        }

        bool alreadyAssigned = await context.UserRoles
            .AnyAsync(ur => ur.UserId == command.UserId && ur.RoleId == command.RoleId, cancellationToken);

        if (alreadyAssigned)
        {
            return Result.Success();
        }

        context.UserRoles.Add(new UserRole { UserId = command.UserId, RoleId = command.RoleId });
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

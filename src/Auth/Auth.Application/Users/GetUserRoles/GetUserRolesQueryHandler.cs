using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Auth.Application.Users.GetUserRoles;

internal sealed class GetUserRolesQueryHandler(IAuthDbContext context)
    : IQueryHandler<GetUserRolesQuery, string[]>
{
    public async Task<Result<string[]>> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        string[] roles = await context.UserRoles
            .Where(ur => ur.UserId == query.UserId)
            .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToArrayAsync(cancellationToken);

        return Result.Success(roles);
    }
}

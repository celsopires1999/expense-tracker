using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Auth.Application.Users.GetAll;

internal sealed class GetAllUsersQueryHandler(IAuthDbContext context)
    : IQueryHandler<GetAllUsersQuery, ListUsersResponse[]>
{
    public async Task<Result<ListUsersResponse[]>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        ListUsersResponse[] users = await context.Users
            .Select(u => new ListUsersResponse
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .ToArrayAsync(cancellationToken);

        Guid[] userIds = [.. users.Select(u => u.Id)];

        Dictionary<Guid, string[]> rolesByUser = await context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Name).ToArray(), cancellationToken);

        foreach (ListUsersResponse user in users)
        {
            user.Roles = rolesByUser.GetValueOrDefault(user.Id, []);
        }

        return Result.Success(users);
    }
}

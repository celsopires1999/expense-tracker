using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using SharedKernel;

namespace Permission.Application.Roles.Resolve;

internal sealed class ResolvePermissionsQueryHandler(IPermissionDbContext context)
    : IQueryHandler<ResolvePermissionsQuery, HashSet<string>>
{
    public async Task<Result<HashSet<string>>> Handle(ResolvePermissionsQuery query, CancellationToken cancellationToken)
    {
        if (query.Roles.Length == 0)
        {
            return Result.Success(new HashSet<string>());
        }

        List<string> permissions = await context.RolePermissions
            .Where(rp => context.Roles
                .Where(r => query.Roles.Contains(r.Name))
                .Select(r => r.Id)
                .Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync(cancellationToken);

        return Result.Success(new HashSet<string>(permissions));
    }
}

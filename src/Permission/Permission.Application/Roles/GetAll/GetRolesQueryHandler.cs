using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using SharedKernel;

namespace Permission.Application.Roles.GetAll;

internal sealed class GetRolesQueryHandler(IPermissionDbContext context)
    : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    public async Task<Result<List<RoleResponse>>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        List<RoleResponse> roles = await context.Roles
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}

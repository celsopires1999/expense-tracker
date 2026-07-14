using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using SharedKernel;

namespace Permission.Application.Roles.GetAll;

internal sealed class GetRoleByIdQueryHandler(IPermissionDbContext context)
    : IQueryHandler<GetRoleByIdQuery, RoleDetailResponse>
{
    public async Task<Result<RoleDetailResponse>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        RoleDetailResponse? role = await context.Roles
            .Where(r => r.Id == query.RoleId)
            .Select(r => new RoleDetailResponse
            {
                Id = r.Id,
                Name = r.Name,
                Permissions = context.RolePermissions
                    .Where(rp => rp.RoleId == r.Id)
                    .Select(rp => rp.Permission)
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleDetailResponse>(Error.NotFound("Role.NotFound", $"Role with Id '{query.RoleId}' was not found"));
        }

        return role;
    }
}

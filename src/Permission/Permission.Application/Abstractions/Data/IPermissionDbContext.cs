using Microsoft.EntityFrameworkCore;
using Permission.Domain.Roles;

namespace Permission.Application.Abstractions.Data;

public interface IPermissionDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

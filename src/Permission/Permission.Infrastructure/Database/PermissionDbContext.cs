using MassTransit;
using Microsoft.EntityFrameworkCore;
using Permission.Application.Abstractions.Data;
using Permission.Domain.Roles;

namespace Permission.Infrastructure.Database;

public sealed class PermissionDbContext(DbContextOptions<PermissionDbContext> options)
    : DbContext(options), IPermissionDbContext
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PermissionDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

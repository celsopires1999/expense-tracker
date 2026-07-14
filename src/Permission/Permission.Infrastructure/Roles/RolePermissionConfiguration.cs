using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Permission.Domain.Roles;

namespace Permission.Infrastructure.Roles;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => new { rp.RoleId, rp.Permission });

        builder.Property(rp => rp.Permission)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(rp => rp.RoleId);

        builder.HasData(
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:create" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:read" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:read:all" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:update" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:update:all" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:delete" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "expenses:delete:all" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "categories:create" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "categories:read" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "categories:update" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "categories:delete" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "payment-methods:create" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "payment-methods:read" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "payment-methods:update" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "payment-methods:delete" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "tags:create" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "tags:read" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "tags:update" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "tags:delete" },
            new RolePermission { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), Permission = "users:access" },
            new RolePermission { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), Permission = "expenses:read" },
            new RolePermission { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), Permission = "expenses:read:all" },
            new RolePermission { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), Permission = "categories:read" },
            new RolePermission { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), Permission = "payment-methods:read" },
            new RolePermission { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), Permission = "tags:read" },
            new RolePermission { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), Permission = "users:access" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "expenses:create" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "expenses:read" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "expenses:update" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "expenses:delete" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "categories:read" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "payment-methods:read" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "tags:read" },
            new RolePermission { RoleId = new Guid("33333333-3333-3333-3333-333333333333"), Permission = "users:access" });
    }
}

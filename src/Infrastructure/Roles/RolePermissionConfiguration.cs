using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Roles;

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
    }
}

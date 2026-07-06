using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Roles;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(
            new Role { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Admin" },
            new Role { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Viewer" },
            new Role { Id = new Guid("33333333-3333-3333-3333-333333333333"), Name = "Standard" });
    }
}

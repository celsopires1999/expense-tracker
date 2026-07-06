using Domain.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PaymentMethods;

internal sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Name).IsUnique();

        builder.HasData(
            new PaymentMethod { Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Crédito" },
            new PaymentMethod { Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Débito" },
            new PaymentMethod { Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Dinheiro" },
            new PaymentMethod { Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Pix" },
            new PaymentMethod { Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Boleto" });
    }
}

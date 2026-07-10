using Expense.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense.Infrastructure.Categories;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new Category { Id = new Guid("33333333-3333-3333-3333-333333333333"), Name = "Alimentação" },
            new Category { Id = new Guid("44444444-4444-4444-4444-444444444444"), Name = "Transporte" },
            new Category { Id = new Guid("55555555-5555-5555-5555-555555555555"), Name = "Moradia" },
            new Category { Id = new Guid("66666666-6666-6666-6666-666666666666"), Name = "Saúde" },
            new Category { Id = new Guid("77777777-7777-7777-7777-777777777777"), Name = "Lazer" },
            new Category { Id = new Guid("88888888-8888-8888-8888-888888888888"), Name = "Educação" });
    }
}

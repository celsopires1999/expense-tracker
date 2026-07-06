using Domain.Expenses;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Expenses;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Date)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

        builder.HasOne<Domain.Categories.Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId);

        builder.HasOne<Domain.PaymentMethods.PaymentMethod>()
            .WithMany()
            .HasForeignKey(e => e.PaymentMethodId);

        builder.HasMany(e => e.Tags)
            .WithOne()
            .HasForeignKey(et => et.ExpenseId);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);
    }
}

internal sealed class ExpenseTagConfiguration : IEntityTypeConfiguration<ExpenseTag>
{
    public void Configure(EntityTypeBuilder<ExpenseTag> builder)
    {
        builder.HasKey(et => new { et.ExpenseId, et.TagId });

        builder.HasOne(et => et.Tag)
            .WithMany()
            .HasForeignKey(et => et.TagId);
    }
}

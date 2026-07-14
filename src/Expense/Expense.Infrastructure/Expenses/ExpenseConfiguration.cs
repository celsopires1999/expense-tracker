using Expense.Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense.Infrastructure.Expenses;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense.Domain.Expenses.Expense>
{
    public void Configure(EntityTypeBuilder<Expense.Domain.Expenses.Expense> builder)
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

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<Domain.Categories.Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId);

        builder.HasOne<Domain.PaymentMethods.PaymentMethod>()
            .WithMany()
            .HasForeignKey(e => e.PaymentMethodId);

        builder.HasMany(e => e.Tags)
            .WithOne(et => et.Expense)
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

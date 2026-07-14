using Expense.Domain.Categories;
using Expense.Domain.Expenses;
using Expense.Domain.PaymentMethods;
using Expense.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Expense.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Expense.Domain.Expenses.Expense> Expenses { get; }
    DbSet<ExpenseTag> ExpenseTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Expenses;

namespace Expense.Application.Expenses.Get;

public sealed class GetExpensesQuery : IQuery<List<ExpenseResponse>>
{
    public Guid? UserId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public ExpenseStatus? Status { get; set; }
}

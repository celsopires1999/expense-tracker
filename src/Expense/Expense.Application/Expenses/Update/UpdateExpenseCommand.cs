using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Expenses.Update;

public sealed class UpdateExpenseCommand : ICommand
{
    public Guid ExpenseId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public List<Guid> TagIds { get; set; } = [];
}

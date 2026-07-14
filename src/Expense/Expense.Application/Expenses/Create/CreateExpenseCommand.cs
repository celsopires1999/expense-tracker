using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Expenses.Create;

public sealed class CreateExpenseCommand : ICommand<Guid>
{
    public Guid UserId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public List<Guid> TagIds { get; set; } = [];
}

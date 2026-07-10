using SharedKernel;

namespace Expense.Domain.Expenses;

public sealed class Expense : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public List<ExpenseTag> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class ExpenseTag
{
    public Guid ExpenseId { get; set; }
    public Expense Expense { get; set; }
    public Guid TagId { get; set; }
    public Tags.Tag Tag { get; set; }
}

using Application.Abstractions.Messaging;

namespace Application.Expenses.Get;

public sealed class GetExpensesQuery : IQuery<List<ExpenseResponse>>
{
    public Guid? UserId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
}

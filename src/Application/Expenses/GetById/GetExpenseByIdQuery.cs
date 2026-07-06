using Application.Abstractions.Messaging;

namespace Application.Expenses.GetById;

public sealed record GetExpenseByIdQuery(Guid ExpenseId) : IQuery<ExpenseResponse>;

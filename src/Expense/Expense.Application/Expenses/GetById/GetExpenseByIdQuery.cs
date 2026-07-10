using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Expenses.GetById;

public sealed record GetExpenseByIdQuery(Guid ExpenseId) : IQuery<ExpenseResponse>;

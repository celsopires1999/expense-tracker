using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Expenses.Delete;

public sealed record DeleteExpenseCommand(Guid ExpenseId) : ICommand;

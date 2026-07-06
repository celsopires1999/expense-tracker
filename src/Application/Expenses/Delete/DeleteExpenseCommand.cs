using Application.Abstractions.Messaging;

namespace Application.Expenses.Delete;

public sealed record DeleteExpenseCommand(Guid ExpenseId) : ICommand;

using SharedKernel;

namespace Expense.Domain.Expenses;

public sealed record ExpenseDeletedDomainEvent(Guid ExpenseId) : IDomainEvent;

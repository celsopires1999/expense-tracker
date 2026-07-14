using SharedKernel;

namespace Expense.Domain.Expenses;

public sealed record ExpenseUpdatedDomainEvent(Guid ExpenseId) : IDomainEvent;

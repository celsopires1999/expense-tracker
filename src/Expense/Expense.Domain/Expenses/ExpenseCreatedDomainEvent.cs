using SharedKernel;

namespace Expense.Domain.Expenses;

public sealed record ExpenseCreatedDomainEvent(Guid ExpenseId) : IDomainEvent;

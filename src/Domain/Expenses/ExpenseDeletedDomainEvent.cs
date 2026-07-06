using SharedKernel;

namespace Domain.Expenses;

public sealed record ExpenseDeletedDomainEvent(Guid ExpenseId) : IDomainEvent;

using SharedKernel;

namespace Domain.Expenses;

public sealed record ExpenseUpdatedDomainEvent(Guid ExpenseId) : IDomainEvent;

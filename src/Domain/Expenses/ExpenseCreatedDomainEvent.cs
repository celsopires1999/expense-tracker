using SharedKernel;

namespace Domain.Expenses;

public sealed record ExpenseCreatedDomainEvent(Guid ExpenseId) : IDomainEvent;

using SharedKernel;

namespace Permission.Domain.Roles;

public sealed record RoleDeletedDomainEvent(Guid RoleId) : IDomainEvent;

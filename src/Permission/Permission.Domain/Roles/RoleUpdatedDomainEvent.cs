using SharedKernel;

namespace Permission.Domain.Roles;

public sealed record RoleUpdatedDomainEvent(Guid RoleId) : IDomainEvent;

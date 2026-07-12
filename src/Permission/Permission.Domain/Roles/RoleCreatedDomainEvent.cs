using SharedKernel;

namespace Permission.Domain.Roles;

public sealed record RoleCreatedDomainEvent(Guid RoleId) : IDomainEvent;

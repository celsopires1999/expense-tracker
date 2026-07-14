namespace SharedKernel.Messaging;

public sealed record RoleDeletedEvent(Guid Id, DateTime DeletedOn);

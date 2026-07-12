namespace SharedKernel.Messaging;

public sealed record RoleUpdatedEvent(Guid Id, string Name, DateTime UpdatedOn);

namespace SharedKernel.Messaging;

public sealed record RoleCreatedEvent(Guid Id, string Name, DateTime CreatedOn);

using Application.Abstractions.Messaging;

namespace Application.Tags.Update;

public sealed record UpdateTagCommand(Guid Id, string Name) : ICommand;

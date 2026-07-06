using Application.Abstractions.Messaging;

namespace Application.Tags.Create;

public sealed record CreateTagCommand(string Name) : ICommand<Guid>;

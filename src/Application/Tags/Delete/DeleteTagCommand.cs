using Application.Abstractions.Messaging;

namespace Application.Tags.Delete;

public sealed record DeleteTagCommand(Guid Id) : ICommand;

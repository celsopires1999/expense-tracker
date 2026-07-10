using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.Create;

public sealed record CreateRoleCommand(string Name) : ICommand<Guid>;

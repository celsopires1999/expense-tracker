using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.Update;

public sealed record UpdateRoleCommand(Guid RoleId, string Name) : ICommand;

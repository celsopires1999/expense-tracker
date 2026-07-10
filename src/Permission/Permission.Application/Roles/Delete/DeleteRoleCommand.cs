using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.Delete;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;

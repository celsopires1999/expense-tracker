using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, Guid RoleId) : ICommand;

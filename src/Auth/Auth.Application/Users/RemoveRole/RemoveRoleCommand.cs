using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, Guid RoleId) : ICommand;

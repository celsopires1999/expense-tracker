using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.Update;

public sealed record UpdateRolePermissionsCommand(Guid RoleId, List<string> Permissions) : ICommand;

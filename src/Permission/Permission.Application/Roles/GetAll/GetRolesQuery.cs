using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.GetAll;

public sealed record GetRolesQuery : IQuery<List<RoleResponse>>;

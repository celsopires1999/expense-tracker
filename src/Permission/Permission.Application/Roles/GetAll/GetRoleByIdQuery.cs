using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.GetAll;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleDetailResponse>;

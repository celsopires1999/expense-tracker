using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<string[]>;

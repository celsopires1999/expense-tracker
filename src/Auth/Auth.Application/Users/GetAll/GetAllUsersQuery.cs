using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetAll;

public sealed record GetAllUsersQuery : IQuery<ListUsersResponse[]>;

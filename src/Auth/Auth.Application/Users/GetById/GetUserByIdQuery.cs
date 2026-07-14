using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;

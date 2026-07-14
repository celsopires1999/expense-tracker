using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserResponse>;

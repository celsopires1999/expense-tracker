using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password)
    : ICommand<Guid>;

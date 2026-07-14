using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<string>;

using Auth.Domain.Users;

namespace Auth.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user, string[] roles);
}

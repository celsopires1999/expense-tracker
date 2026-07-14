namespace Expense.Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    string[] Roles { get; }

    bool IsInRole(string role);
}

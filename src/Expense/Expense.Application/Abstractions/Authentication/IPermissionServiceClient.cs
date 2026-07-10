namespace Expense.Application.Abstractions.Authentication;

public interface IPermissionServiceClient
{
    Task<HashSet<string>> ResolvePermissionsAsync(string[] roles, CancellationToken cancellationToken = default);
}

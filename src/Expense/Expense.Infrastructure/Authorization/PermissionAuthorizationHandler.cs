using System.Security.Claims;
using Expense.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Expense.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(
    IPermissionServiceClient permissionServiceClient,
    IMemoryCache cache) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is not { Identity.IsAuthenticated: true })
        {
            return;
        }

        Guid userId = GetUserId(context.User);
        string[] roles = GetRoles(context.User);

        string cacheKey = $"perms:{userId}";

        HashSet<string>? permissions = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                return await permissionServiceClient.ResolvePermissionsAsync(roles);
            });

        if (permissions is not null && permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }

    private static Guid GetUserId(ClaimsPrincipal principal)
    {
        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out Guid parsedUserId) ? parsedUserId : Guid.Empty;
    }

    private static string[] GetRoles(ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }
}

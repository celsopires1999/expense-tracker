using System.Security.Claims;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory, IMemoryCache cache)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is not { Identity.IsAuthenticated: true })
        {
            return;
        }

        Guid userId = context.User.GetUserId();
        int version = GetPermissionVersion(context.User);

        string cacheKey = $"perms:{userId}:v{version}";

        HashSet<string>? permissions = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                using IServiceScope scope = serviceScopeFactory.CreateScope();
                PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

                return await permissionProvider.GetForUserIdAsync(userId);
            });

        if (permissions is not null && permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }

    private static int GetPermissionVersion(ClaimsPrincipal principal)
    {
        string? versionClaim = principal.FindFirstValue("perm_version");

        return int.TryParse(versionClaim, out int version) ? version : 0;
    }
}

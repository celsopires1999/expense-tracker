using System.Security.Claims;
using Expense.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Expense.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;

            string? userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out Guid userId) ? userId : Guid.Empty;
        }
    }
}

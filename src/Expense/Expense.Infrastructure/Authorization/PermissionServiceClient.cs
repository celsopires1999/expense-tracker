using System.Net.Http.Json;
using Expense.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;

namespace Expense.Infrastructure.Authorization;

internal sealed class PermissionServiceClient(HttpClient httpClient, IConfiguration configuration)
    : IPermissionServiceClient
{
    public async Task<HashSet<string>> ResolvePermissionsAsync(string[] roles, CancellationToken cancellationToken = default)
    {
        string baseUrl = configuration["PermissionService:BaseUrl"]!;

        var request = new { Roles = roles };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{baseUrl}/permissions/resolve",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        HashSet<string>? permissions = await response.Content.ReadFromJsonAsync<HashSet<string>>(cancellationToken: cancellationToken);

        return permissions ?? [];
    }
}

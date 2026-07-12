using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Permission.IntegrationTests.RoleSync;

[Collection("RoleSync")]
public sealed class RoleSyncTests : IClassFixture<RoleSyncFixture>, IAsyncLifetime, IDisposable
{
    private readonly RoleSyncFixture _fixture;
    private readonly RoleSyncPermissionApiFactory _permissionFactory;
    private readonly RoleSyncAuthApiFactory _authFactory;
    private readonly HttpClient _permissionClient;

    public RoleSyncTests(RoleSyncFixture fixture)
    {
        _fixture = fixture;
        _permissionFactory = new RoleSyncPermissionApiFactory(fixture);
        _authFactory = new RoleSyncAuthApiFactory(fixture);
        _permissionClient = CreateAuthenticatedClient();
    }

    public async Task InitializeAsync()
    {
        await _permissionFactory.InitializeDatabaseAsync();
        await _authFactory.InitializeDatabaseAsync();
        await _permissionFactory.CleanDatabaseAsync();
        await _authFactory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        _permissionFactory.Dispose();
        _authFactory.Dispose();
    }

    [Fact]
    public async Task CreateRole_ShouldSyncToAuthDatabase()
    {
        var body = new { Name = "SyncedRole", Permissions = Array.Empty<string>() };
        HttpResponseMessage response = await _permissionClient.PostAsJsonAsync("permissions/roles", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        string? locationHeader = response.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);
        string roleIdStr = locationHeader.Split('/')[^1];
        var roleId = Guid.Parse(roleIdStr);

        Auth.Domain.Roles.Role? authRole = await PollUntilAsync(
            () => FindAuthRoleAsync(roleId),
            role => role is not null,
            timeoutSeconds: 10);

        Assert.NotNull(authRole);
        Assert.Equal("SyncedRole", authRole.Name);
    }

    [Fact]
    public async Task UpdateRole_ShouldSyncNameToAuthDatabase()
    {
        var createBody = new { Name = "OriginalName", Permissions = Array.Empty<string>() };
        HttpResponseMessage createResponse = await _permissionClient.PostAsJsonAsync("permissions/roles", createBody);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        string? locationHeader = createResponse.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);
        string roleIdStr = locationHeader.Split('/')[^1];
        var roleId = Guid.Parse(roleIdStr);

        await PollUntilAsync(
            () => FindAuthRoleAsync(roleId),
            role => role is not null,
            timeoutSeconds: 10);

        var updateBody = new { Name = "UpdatedName" };
        HttpResponseMessage updateResponse = await _permissionClient.PutAsJsonAsync(
            $"permissions/roles/{roleId}", updateBody);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        Auth.Domain.Roles.Role? authRole = await PollUntilAsync(
            () => FindAuthRoleByNameAsync(roleId, "UpdatedName"),
            role => role is not null,
            timeoutSeconds: 10);

        Assert.NotNull(authRole);
        Assert.Equal("UpdatedName", authRole.Name);
    }

    [Fact]
    public async Task DeleteRole_ShouldRemoveFromAuthDatabase()
    {
        var createBody = new { Name = "ToDeleteRole", Permissions = Array.Empty<string>() };
        HttpResponseMessage createResponse = await _permissionClient.PostAsJsonAsync("permissions/roles", createBody);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        string? locationHeader = createResponse.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);
        string roleIdStr = locationHeader.Split('/')[^1];
        var roleId = Guid.Parse(roleIdStr);

        await PollUntilAsync(
            () => FindAuthRoleAsync(roleId),
            role => role is not null,
            timeoutSeconds: 10);

        HttpResponseMessage deleteResponse = await _permissionClient.DeleteAsync($"permissions/roles/{roleId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await PollUntilConditionAsync(
            () => VerifyRoleDeletedAsync(roleId),
            timeoutSeconds: 30);
    }

    private HttpClient CreateAuthenticatedClient()
    {
        HttpClient client = _permissionFactory.CreateClient();
        string token = CreateJwtToken();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private string CreateJwtToken()
    {
        var credentials = new SigningCredentials(_fixture.TestSecurityKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "test@example.com"),
            new(ClaimTypes.Role, "Standard")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = credentials,
            Issuer = "auth-service",
            Audience = "expense-tracker"
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    private async Task<Auth.Domain.Roles.Role?> FindAuthRoleAsync(Guid roleId)
    {
        using IServiceScope scope = _authFactory.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        return await context.Roles.FindAsync(roleId);
    }

    private async Task<Auth.Domain.Roles.Role?> FindAuthRoleByNameAsync(Guid roleId, string expectedName)
    {
        using IServiceScope scope = _authFactory.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        Auth.Domain.Roles.Role? role = await context.Roles.FindAsync(roleId);
        return role?.Name == expectedName ? role : null;
    }

    private async Task<bool> VerifyRoleDeletedAsync(Guid roleId)
    {
        using IServiceScope scope = _authFactory.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        Auth.Domain.Roles.Role? role = await context.Roles.FindAsync(roleId);
        return role is null;
    }

    private static async Task<T> PollUntilAsync<T>(
        Func<Task<T?>> action,
        Func<T?, bool> condition,
        int timeoutSeconds) where T : class
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        T? result = null;

        while (DateTime.UtcNow < deadline)
        {
            result = await action();
            if (condition(result))
            {
                return result;
            }

            await Task.Delay(250);
        }

        return result!;
    }

    private static async Task PollUntilConditionAsync(
        Func<Task<bool>> condition,
        int timeoutSeconds)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Condition not met within {timeoutSeconds} seconds");
    }
}

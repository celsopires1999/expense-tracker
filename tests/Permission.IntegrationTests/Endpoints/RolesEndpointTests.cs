using System.Net;
using System.Net.Http.Json;
using Permission.Application.Roles.GetAll;

namespace Permission.IntegrationTests.Endpoints;

[Collection("Permission")]
public sealed class RolesEndpointTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;
    private readonly HttpClient _client;

    public RolesEndpointTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
        _client = PermissionTestHelper.CreateAuthenticatedClient(_factory);
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturn200_WithRoles()
    {
        HttpResponseMessage response = await _client.GetAsync("permissions/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<object>? roles = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(roles);
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WithoutToken()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();

        HttpResponseMessage response = await unauthenticatedClient.GetAsync("permissions/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WithRoleId()
    {
        object body = new { Name = "New Role", Permissions = Array.Empty<string>() };

        HttpResponseMessage response = await _client.PostAsJsonAsync("permissions/roles", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldCreateRole_WithAllProperties()
    {
        string roleName = "Manager";
        string[] permissions =
        [
            "expenses:create",
            "expenses:read",
            "expenses:update",
            "expenses:delete",
            "categories:create",
            "categories:read",
            "categories:update",
            "categories:delete",
            "payment-methods:read",
            "tags:read",
            "users:access"
        ];

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(
            "permissions/roles",
            new { Name = roleName, Permissions = permissions });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        string? locationHeader = createResponse.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);
        string roleId = locationHeader.Split('/')[^1];

        HttpResponseMessage getResponse = await _client.GetAsync($"permissions/roles/{roleId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        RoleDetailResponse? role = await getResponse.Content.ReadFromJsonAsync<RoleDetailResponse>();
        Assert.NotNull(role);
        Assert.Equal(roleName, role.Name);
        Assert.Equal(permissions.Length, role.Permissions.Count);
        Assert.Contains("expenses:create", role.Permissions);
        Assert.Contains("expenses:read", role.Permissions);
        Assert.Contains("expenses:update", role.Permissions);
        Assert.Contains("expenses:delete", role.Permissions);
        Assert.Contains("categories:create", role.Permissions);
        Assert.Contains("categories:read", role.Permissions);
        Assert.Contains("categories:update", role.Permissions);
        Assert.Contains("categories:delete", role.Permissions);
        Assert.Contains("payment-methods:read", role.Permissions);
        Assert.Contains("tags:read", role.Permissions);
        Assert.Contains("users:access", role.Permissions);
    }

    [Fact]
    public async Task Create_ShouldCreateRole_WithoutPermissions()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(
            "permissions/roles",
            new { Name = "ReadOnly", Permissions = Array.Empty<string>() });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        string? locationHeader = createResponse.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);
        string roleId = locationHeader.Split('/')[^1];

        HttpResponseMessage getResponse = await _client.GetAsync($"permissions/roles/{roleId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        RoleDetailResponse? role = await getResponse.Content.ReadFromJsonAsync<RoleDetailResponse>();
        Assert.NotNull(role);
        Assert.Equal("ReadOnly", role.Name);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenExists()
    {
        var roleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        HttpResponseMessage response = await _client.GetAsync($"permissions/roles/{roleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_ForUnknownId()
    {
        HttpResponseMessage response = await _client.GetAsync($"permissions/roles/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenExists()
    {
        object body = new { Name = "ToDelete", Permissions = Array.Empty<string>() };
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("permissions/roles", body);
        string? locationHeader = createResponse.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);
        string roleId = locationHeader.Split('/')[^1];

        HttpResponseMessage response = await _client.DeleteAsync($"permissions/roles/{roleId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePermissions_ShouldReturn200_WhenExists()
    {
        var roleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        string[] permissions = ["expenses:read", "expenses:create"];

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"permissions/roles/{roleId}/permissions",
            permissions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Resolve_ShouldReturn200_Anonymous()
    {
        HttpClient client = _factory.CreateClient();
        string[] roles = ["Standard"];

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "permissions/resolve",
            new { Roles = roles });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        HashSet<string>? permissions = await response.Content.ReadFromJsonAsync<HashSet<string>>();
        Assert.NotNull(permissions);
        Assert.Contains("expenses:create", permissions);
    }
}

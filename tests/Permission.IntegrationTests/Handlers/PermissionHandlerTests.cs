using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Permission.Application.Abstractions.Data;
using Permission.Application.Abstractions.Messaging;
using Permission.Application.Roles.Create;
using Permission.Application.Roles.Delete;
using Permission.Application.Roles.GetAll;
using Permission.Application.Roles.Resolve;
using Permission.Application.Roles.Update;
using Permission.Infrastructure.Database;
using SharedKernel;

namespace Permission.IntegrationTests.Handlers;

[Collection("Permission")]
public sealed class CreateRoleCommandTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;

    public CreateRoleCommandTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldCreateRole_InDatabase()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateRoleCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateRoleCommand, Guid>>();

        var command = new CreateRoleCommand("Moderator", ["test:read", "test:write"]);
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        PermissionDbContext context = verifyScope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        Permission.Domain.Roles.Role? role = await context.Roles.FindAsync(result.Value);

        Assert.NotNull(role);
        Assert.Equal("Moderator", role.Name);

        List<string> permissions = await context.RolePermissions
            .Where(rp => rp.RoleId == result.Value)
            .Select(rp => rp.Permission)
            .ToListAsync();
        Assert.Equal(2, permissions.Count);
        Assert.Contains("test:read", permissions);
        Assert.Contains("test:write", permissions);
    }

    [Fact]
    public async Task Create_ShouldReturnFailure_WhenNameIsEmpty()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<CreateRoleCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateRoleCommand, Guid>>();

        var command = new CreateRoleCommand(string.Empty, []);
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}

[Collection("Permission")]
public sealed class DeleteRoleCommandTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;

    public DeleteRoleCommandTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Delete_ShouldRemoveRole_FromDatabase()
    {
        var roleId = Guid.NewGuid();
        await SeedRoleAsync(roleId, "ToDelete");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<DeleteRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteRoleCommand>>();

        var command = new DeleteRoleCommand(roleId);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        PermissionDbContext context = verifyScope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        Permission.Domain.Roles.Role? role = await context.Roles.FindAsync(roleId);

        Assert.Null(role);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_ForUnknownId()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<DeleteRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteRoleCommand>>();

        var command = new DeleteRoleCommand(Guid.NewGuid());
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Role.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Delete_ShouldRemoveAssociatedPermissions()
    {
        var roleId = Guid.NewGuid();
        await SeedRoleAsync(roleId, "ToDeleteWithPerms");
        await SeedPermissionAsync(roleId, "test:permission");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<DeleteRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteRoleCommand>>();

        var command = new DeleteRoleCommand(roleId);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        PermissionDbContext context = verifyScope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        bool hasPermissions = await context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId);

        Assert.False(hasPermissions);
    }

    private async Task SeedRoleAsync(Guid roleId, string name)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        PermissionDbContext context = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        context.Roles.Add(new Permission.Domain.Roles.Role { Id = roleId, Name = name });
        await context.SaveChangesAsync();
    }

    private async Task SeedPermissionAsync(Guid roleId, string permission)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        PermissionDbContext context = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        context.RolePermissions.Add(new Permission.Domain.Roles.RolePermission
        {
            RoleId = roleId,
            Permission = permission
        });
        await context.SaveChangesAsync();
    }
}

[Collection("Permission")]
public sealed class UpdateRolePermissionsCommandTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;

    public UpdateRolePermissionsCommandTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Update_ShouldReplacePermissions()
    {
        var roleId = Guid.NewGuid();
        await SeedRoleAsync(roleId, "ToUpdate");
        await SeedPermissionAsync(roleId, "old:permission");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<UpdateRolePermissionsCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateRolePermissionsCommand>>();

        var command = new UpdateRolePermissionsCommand(roleId, ["new:permission1", "new:permission2"]);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using IServiceScope verifyScope = _factory.Services.CreateScope();
        PermissionDbContext context = verifyScope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        List<string> permissions = await context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();

        Assert.Equal(2, permissions.Count);
        Assert.Contains("new:permission1", permissions);
        Assert.Contains("new:permission2", permissions);
        Assert.DoesNotContain("old:permission", permissions);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_ForUnknownRole()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<UpdateRolePermissionsCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateRolePermissionsCommand>>();

        var command = new UpdateRolePermissionsCommand(Guid.NewGuid(), ["test:perm"]);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Role.NotFound", result.Error.Code);
    }

    private async Task SeedRoleAsync(Guid roleId, string name)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        PermissionDbContext context = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        context.Roles.Add(new Permission.Domain.Roles.Role { Id = roleId, Name = name });
        await context.SaveChangesAsync();
    }

    private async Task SeedPermissionAsync(Guid roleId, string permission)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        PermissionDbContext context = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
        context.RolePermissions.Add(new Permission.Domain.Roles.RolePermission
        {
            RoleId = roleId,
            Permission = permission
        });
        await context.SaveChangesAsync();
    }
}

[Collection("Permission")]
public sealed class GetRolesQueryTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;

    public GetRolesQueryTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturnSeededRoles()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetRolesQuery, List<RoleResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRolesQuery, List<RoleResponse>>>();

        var query = new GetRolesQuery();
        Result<List<RoleResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOrderedByName()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetRolesQuery, List<RoleResponse>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRolesQuery, List<RoleResponse>>>();

        var query = new GetRolesQuery();
        Result<List<RoleResponse>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var names = result.Value.Select(r => r.Name).ToList();
        Assert.Equal(3, names.Count);
        Assert.Contains("Admin", names);
        Assert.Contains("Standard", names);
        Assert.Contains("Viewer", names);
    }
}

[Collection("Permission")]
public sealed class GetRoleByIdQueryTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;

    public GetRoleByIdQueryTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_ShouldReturnRoleWithPermissions()
    {
        var roleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetRoleByIdQuery, RoleDetailResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRoleByIdQuery, RoleDetailResponse>>();

        var query = new GetRoleByIdQuery(roleId);
        Result<RoleDetailResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Standard", result.Value.Name);
        Assert.NotEmpty(result.Value.Permissions);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_ForUnknownId()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetRoleByIdQuery, RoleDetailResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRoleByIdQuery, RoleDetailResponse>>();

        var query = new GetRoleByIdQuery(Guid.NewGuid());
        Result<RoleDetailResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Role.NotFound", result.Error.Code);
    }
}

[Collection("Permission")]
public sealed class ResolvePermissionsQueryTests : IClassFixture<PermissionApiFixture>, IAsyncLifetime
{
    private readonly PermissionApiFactory _factory;

    public ResolvePermissionsQueryTests(PermissionApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Resolve_ShouldReturnPermissions_ForKnownRoles()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<ResolvePermissionsQuery, HashSet<string>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<ResolvePermissionsQuery, HashSet<string>>>();

        var query = new ResolvePermissionsQuery(["Standard"]);
        Result<HashSet<string>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("expenses:create", result.Value);
        Assert.Contains("expenses:read", result.Value);
    }

    [Fact]
    public async Task Resolve_ShouldReturnEmpty_WhenNoRoles()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<ResolvePermissionsQuery, HashSet<string>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<ResolvePermissionsQuery, HashSet<string>>>();

        var query = new ResolvePermissionsQuery([]);
        Result<HashSet<string>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Resolve_ShouldDeduplicatePermissions_AcrossRoles()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<ResolvePermissionsQuery, HashSet<string>> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<ResolvePermissionsQuery, HashSet<string>>>();

        var query = new ResolvePermissionsQuery(["Admin", "Standard"]);
        Result<HashSet<string>> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("expenses:create", result.Value);
    }
}

using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.AssignRole;
using Auth.Application.Users.Register;
using Auth.Domain.Roles;
using Auth.Domain.Users;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class AssignRoleCommandTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public AssignRoleCommandTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Assign_ShouldCreateUserRole_InDatabase()
    {
        Guid userId = await CreateUserAsync("assign@example.com");
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<AssignRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<AssignRoleCommand>>();

        AssignRoleCommand command = new(userId, roleId);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        UserRole? userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        Assert.NotNull(userRole);
    }

    [Fact]
    public async Task Assign_ShouldBeIdempotent_ForDoubleAssign()
    {
        Guid userId = await CreateUserAsync("idempotent@example.com");
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<AssignRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<AssignRoleCommand>>();

        AssignRoleCommand command = new(userId, roleId);
        Result firstResult = await handler.Handle(command, CancellationToken.None);
        Result secondResult = await handler.Handle(command, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);

        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        int count = await context.UserRoles.CountAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Assign_ShouldReturnError_ForUnknownUser()
    {
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<AssignRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<AssignRoleCommand>>();

        AssignRoleCommand command = new(Guid.NewGuid(), roleId);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Assign_ShouldReturnError_ForUnknownRole()
    {
        Guid userId = await CreateUserAsync("unknownrole@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<AssignRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<AssignRoleCommand>>();

        AssignRoleCommand command = new(userId, Guid.NewGuid());
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.RoleNotFound.Code, result.Error.Code);
    }

    private async Task<Guid> CreateUserAsync(string email)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        RegisterUserCommand command = new(email, "John", "Doe", "Password123!");
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);
        return result.Value;
    }
}

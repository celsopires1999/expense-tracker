using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.AssignRole;
using Auth.Application.Users.Register;
using Auth.Application.Users.RemoveRole;
using Auth.Domain.Roles;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class RemoveRoleCommandTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public RemoveRoleCommandTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Remove_ShouldDeleteUserRole_FromDatabase()
    {
        Guid userId = await CreateUserAsync("remove@example.com");
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using IServiceScope assignScope = _factory.Services.CreateScope();
        ICommandHandler<AssignRoleCommand> assignHandler =
            assignScope.ServiceProvider.GetRequiredService<ICommandHandler<AssignRoleCommand>>();
        await assignHandler.Handle(new AssignRoleCommand(userId, roleId), CancellationToken.None);

        using IServiceScope removeScope = _factory.Services.CreateScope();
        ICommandHandler<RemoveRoleCommand> removeHandler =
            removeScope.ServiceProvider.GetRequiredService<ICommandHandler<RemoveRoleCommand>>();

        RemoveRoleCommand command = new(userId, roleId);
        Result result = await removeHandler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        AuthDbContext context = removeScope.ServiceProvider.GetRequiredService<AuthDbContext>();
        UserRole? userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        Assert.Null(userRole);
    }

    [Fact]
    public async Task Remove_ShouldBeIdempotent_ForNonExistentRole()
    {
        Guid userId = await CreateUserAsync("idempotent-remove@example.com");
        var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RemoveRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RemoveRoleCommand>>();

        RemoveRoleCommand command = new(userId, roleId);
        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
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

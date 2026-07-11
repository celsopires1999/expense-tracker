using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.Register;
using Auth.Domain.Roles;
using Auth.Domain.Users;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class RegisterUserCommandTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public RegisterUserCommandTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldCreateUser_InDatabase()
    {
        RegisterUserCommand command = new("new@example.com", "John", "Doe", "Password123!");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == result.Value);

        Assert.NotNull(user);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
    }

    [Fact]
    public async Task Create_ShouldHashPassword_InDatabase()
    {
        RegisterUserCommand command = new("hash@example.com", "Jane", "Doe", "Password123!");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == result.Value);

        Assert.NotNull(user);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.Contains("-", user.PasswordHash);
    }

    [Fact]
    public async Task Create_ShouldAssignStandardRole_ToNewUser()
    {
        RegisterUserCommand command = new("role@example.com", "Role", "User", "Password123!");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        UserRole? userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == result.Value);

        Assert.NotNull(userRole);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), userRole.RoleId);
    }

    [Fact]
    public async Task Create_ShouldRejectDuplicateEmail()
    {
        RegisterUserCommand command = new("duplicate@example.com", "First", "User", "Password123!");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> firstResult = await handler.Handle(command, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);

        RegisterUserCommand duplicateCommand = new("duplicate@example.com", "Second", "User", "Password123!");
        Result<Guid> secondResult = await handler.Handle(duplicateCommand, CancellationToken.None);

        Assert.True(secondResult.IsFailure);
        Assert.Equal(UserErrors.EmailNotUnique.Code, secondResult.Error.Code);
    }

    [Fact]
    public async Task Create_ShouldRejectEmptyEmail()
    {
        RegisterUserCommand command = new("", "John", "Doe", "Password123!");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_ShouldRejectShortPassword()
    {
        RegisterUserCommand command = new("short@example.com", "John", "Doe", "123");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_ShouldRejectEmptyFirstName()
    {
        RegisterUserCommand command = new("empty@example.com", "", "Doe", "Password123!");

        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<RegisterUserCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}

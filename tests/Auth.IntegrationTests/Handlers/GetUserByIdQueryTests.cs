using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users;
using Auth.Application.Users.AssignRole;
using Auth.Application.Users.GetById;
using Auth.Application.Users.Register;
using Auth.Domain.Users;
using Auth.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class GetUserByIdQueryTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public GetUserByIdQueryTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_ShouldReturnUser_WhenExists()
    {
        Guid userId = await CreateUserAsync("get@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetUserByIdQuery, UserResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserByIdQuery, UserResponse>>();

        GetUserByIdQuery query = new(userId);
        Result<UserResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.Id);
        Assert.Equal("get@example.com", result.Value.Email);
        Assert.Equal("John", result.Value.FirstName);
        Assert.Equal("Doe", result.Value.LastName);
    }

    [Fact]
    public async Task GetById_ShouldNotIncludePasswordHash()
    {
        Guid userId = await CreateUserAsync("nohash@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetUserByIdQuery, UserResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserByIdQuery, UserResponse>>();

        GetUserByIdQuery query = new(userId);
        Result<UserResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("PasswordHash", result.Value.GetType().GetProperties().Select(p => p.Name!).ToList());
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_ForUnknownId()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetUserByIdQuery, UserResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserByIdQuery, UserResponse>>();

        GetUserByIdQuery query = new(Guid.NewGuid());
        Result<UserResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task GetById_ShouldIncludeRoles_WhenAssigned()
    {
        Guid userId = await CreateUserAsync("rolesbyid@example.com");
        await AssignRoleAsync(userId, new Guid("11111111-1111-1111-1111-111111111111"));

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetUserByIdQuery, UserResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserByIdQuery, UserResponse>>();

        GetUserByIdQuery query = new(userId);
        Result<UserResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("Admin", result.Value.Roles);
    }

    [Fact]
    public async Task GetById_ShouldReturnDefaultRole_WhenOnlyRegistered()
    {
        Guid userId = await CreateUserAsync("defaultrolebyid@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetUserByIdQuery, UserResponse> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserByIdQuery, UserResponse>>();

        GetUserByIdQuery query = new(userId);
        Result<UserResponse> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("Standard", result.Value.Roles);
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

    private async Task AssignRoleAsync(Guid userId, Guid roleId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<AssignRoleCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<AssignRoleCommand>>();

        AssignRoleCommand command = new(userId, roleId);
        await handler.Handle(command, CancellationToken.None);
    }
}

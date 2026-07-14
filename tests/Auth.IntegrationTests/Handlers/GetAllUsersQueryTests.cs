using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users;
using Auth.Application.Users.AssignRole;
using Auth.Application.Users.GetAll;
using Auth.Application.Users.Register;
using Auth.Domain.Users;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class GetAllUsersQueryTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public GetAllUsersQueryTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturnUsers_WhenExist()
    {
        await CreateUserAsync("first@example.com");
        await CreateUserAsync("second@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetAllUsersQuery, ListUsersResponse[]> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllUsersQuery, ListUsersResponse[]>>();

        Result<ListUsersResponse[]> result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Length);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyArray_WhenNoUsers()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetAllUsersQuery, ListUsersResponse[]> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllUsersQuery, ListUsersResponse[]>>();

        Result<ListUsersResponse[]> result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAll_ShouldIncludeRoles_WhenAssigned()
    {
        Guid userId = await CreateUserAsync("roles@example.com");
        await AssignRoleAsync(userId, new Guid("11111111-1111-1111-1111-111111111111"));

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetAllUsersQuery, ListUsersResponse[]> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllUsersQuery, ListUsersResponse[]>>();

        Result<ListUsersResponse[]> result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        ListUsersResponse user = Assert.Single(result.Value);
        Assert.Contains("Admin", user.Roles);
    }

    [Fact]
    public async Task GetAll_ShouldReturnDefaultRole_WhenOnlyRegistered()
    {
        await CreateUserAsync("defaultrole@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetAllUsersQuery, ListUsersResponse[]> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAllUsersQuery, ListUsersResponse[]>>();

        Result<ListUsersResponse[]> result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        ListUsersResponse user = Assert.Single(result.Value);
        Assert.Contains("Standard", user.Roles);
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

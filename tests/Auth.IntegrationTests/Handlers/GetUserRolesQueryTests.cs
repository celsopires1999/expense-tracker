using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.GetUserRoles;
using Auth.Application.Users.Register;
using Auth.Domain.Roles;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class GetUserRolesQueryTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public GetUserRolesQueryTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetUserRoles_ShouldReturnRoles_ForUser()
    {
        Guid userId = await CreateUserAsync("roles@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        IQueryHandler<GetUserRolesQuery, string[]> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserRolesQuery, string[]>>();

        GetUserRolesQuery query = new(userId);
        Result<string[]> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("Standard", result.Value);
    }

    [Fact]
    public async Task GetUserRoles_ShouldReturnEmpty_ForUserWithNoRoles()
    {
        Guid userId = await CreateUserAsync("noroles@example.com");

        using IServiceScope scope = _factory.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        List<UserRole> userRoles = await context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
        context.UserRoles.RemoveRange(userRoles);
        await context.SaveChangesAsync();

        IQueryHandler<GetUserRolesQuery, string[]> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<GetUserRolesQuery, string[]>>();

        GetUserRolesQuery query = new(userId);
        Result<string[]> result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
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

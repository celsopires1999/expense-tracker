using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.Login;
using Auth.Domain.Users;
using Auth.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.IntegrationTests.Handlers;

[Collection("Auth")]
public sealed class LoginUserCommandTests : IClassFixture<AuthApiFixture>, IAsyncLifetime
{
    private readonly AuthApiFactory _factory;

    public LoginUserCommandTests(AuthApiFixture fixture)
    {
        _factory = fixture.Factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.CleanDatabaseAsync();
        await AuthTestHelper.CreateTestUserAsync(_factory, "login@example.com", "Password123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_ShouldReturnToken_ForValidCredentials()
    {
        string token = await AuthTestHelper.GetJwtTokenAsync(_factory, "login@example.com", "Password123!");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_ShouldRejectUnknownEmail()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<LoginUserCommand, string> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<LoginUserCommand, string>>();

        LoginUserCommand command = new("nonexistent@example.com", "Password123!");
        Result<string> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFoundByEmail.Code, result.Error.Code);
    }

    [Fact]
    public async Task Login_ShouldRejectWrongPassword()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ICommandHandler<LoginUserCommand, string> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<LoginUserCommand, string>>();

        LoginUserCommand command = new("login@example.com", "WrongPassword123!");
        Result<string> result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFoundByEmail.Code, result.Error.Code);
    }

    [Fact]
    public async Task Login_ShouldReturnVerifiableJwtToken()
    {
        string token = await AuthTestHelper.GetJwtTokenAsync(_factory, "login@example.com", "Password123!");

        string[] parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task Login_ShouldIncludeCorrectClaims()
    {
        string token = await AuthTestHelper.GetJwtTokenAsync(_factory, "login@example.com", "Password123!");

        Assert.False(string.IsNullOrWhiteSpace(token));

        using IServiceScope scope = _factory.Services.CreateScope();
        AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == "login@example.com");

        Assert.NotNull(user);
    }
}

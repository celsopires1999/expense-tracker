using Auth.Api.Endpoints;
using Auth.Api.Infrastructure;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.Login;
using SharedKernel;

namespace Auth.Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (LoginUserCommand command, ICommandHandler<LoginUserCommand, string> handler, CancellationToken cancellationToken) =>
        {
            Result<string> result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { Token = result.Value })
                : CustomResults.Problem(result);
        })
        .AllowAnonymous();
    }
}

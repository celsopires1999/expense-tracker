using Auth.Api.Endpoints;
using Auth.Api.Infrastructure;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.Register;
using SharedKernel;

namespace Auth.Api.Endpoints.Users;

internal sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (RegisterUserCommand command, ICommandHandler<RegisterUserCommand, Guid> handler, CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/users/{result.Value}", result.Value)
                : CustomResults.Problem(result);
        })
        .AllowAnonymous();
    }
}

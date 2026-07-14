using Auth.Api.Endpoints;
using Auth.Api.Infrastructure;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.RemoveRole;
using SharedKernel;

namespace Auth.Api.Endpoints.Users;

internal sealed class RemoveRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("auth/users/{userId:guid}/roles/{roleId:guid}", async (Guid userId, Guid roleId, ICommandHandler<RemoveRoleCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new RemoveRoleCommand(userId, roleId);
            Result result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok()
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();
    }
}

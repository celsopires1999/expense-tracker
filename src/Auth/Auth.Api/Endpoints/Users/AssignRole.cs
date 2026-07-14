using Auth.Api.Endpoints;
using Auth.Api.Infrastructure;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.AssignRole;
using SharedKernel;

namespace Auth.Api.Endpoints.Users;

internal sealed class AssignRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/users/{userId:guid}/roles/{roleId:guid}", async (Guid userId, Guid roleId, ICommandHandler<AssignRoleCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new AssignRoleCommand(userId, roleId);
            Result result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok()
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();
    }
}

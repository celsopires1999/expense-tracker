using Auth.Api.Endpoints;
using Auth.Api.Infrastructure;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users;
using Auth.Application.Users.GetById;
using SharedKernel;

namespace Auth.Api.Endpoints.Users;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/users/{userId:guid}", async (Guid userId, IQueryHandler<GetUserByIdQuery, UserResponse> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetUserByIdQuery(userId);
            Result<UserResponse> result = await handler.Handle(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();
    }
}

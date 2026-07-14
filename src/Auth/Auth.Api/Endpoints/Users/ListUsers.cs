using Auth.Api.Endpoints;
using Auth.Api.Infrastructure;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users;
using Auth.Application.Users.GetAll;
using SharedKernel;

namespace Auth.Api.Endpoints.Users;

internal sealed class ListUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/users", async (IQueryHandler<GetAllUsersQuery, ListUsersResponse[]> handler, CancellationToken cancellationToken) =>
        {
            Result<ListUsersResponse[]> result = await handler.Handle(new GetAllUsersQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();
    }
}

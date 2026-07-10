using Permission.Api.Infrastructure;
using Permission.Application.Abstractions.Messaging;
using Permission.Application.Roles.Create;
using Permission.Application.Roles.Delete;
using Permission.Application.Roles.GetAll;
using Permission.Application.Roles.Resolve;
using Permission.Application.Roles.Update;
using SharedKernel;

namespace Permission.Api.Endpoints;

internal sealed class RolesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("permissions/roles", async (CreateRoleCommand command, ICommandHandler<CreateRoleCommand, Guid> handler, CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/permissions/roles/{result.Value}", result.Value)
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();

        app.MapGet("permissions/roles", async (IQueryHandler<GetRolesQuery, List<RoleResponse>> handler, CancellationToken cancellationToken) =>
        {
            Result<List<RoleResponse>> result = await handler.Handle(new GetRolesQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();

        app.MapGet("permissions/roles/{roleId:guid}", async (Guid roleId, IQueryHandler<GetRoleByIdQuery, RoleDetailResponse> handler, CancellationToken cancellationToken) =>
        {
            Result<RoleDetailResponse> result = await handler.Handle(new GetRoleByIdQuery(roleId), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();

        app.MapPut("permissions/roles/{roleId:guid}/permissions", async (Guid roleId, List<string> permissions, ICommandHandler<UpdateRolePermissionsCommand> handler, CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new UpdateRolePermissionsCommand(roleId, permissions), cancellationToken);

            return result.IsSuccess
                ? Results.Ok()
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();

        app.MapDelete("permissions/roles/{roleId:guid}", async (Guid roleId, ICommandHandler<DeleteRoleCommand> handler, CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeleteRoleCommand(roleId), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : CustomResults.Problem(result);
        })
        .RequireAuthorization();

        app.MapPost("permissions/resolve", async (ResolvePermissionsQuery query, IQueryHandler<ResolvePermissionsQuery, HashSet<string>> handler, CancellationToken cancellationToken) =>
        {
            Result<HashSet<string>> result = await handler.Handle(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : CustomResults.Problem(result);
        })
        .AllowAnonymous();
    }
}

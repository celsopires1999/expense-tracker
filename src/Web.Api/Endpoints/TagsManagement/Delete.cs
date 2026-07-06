using Application.Abstractions.Messaging;
using Application.Tags.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.TagsManagement;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("tags/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteTagCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTagCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.TagsManagement)
        .HasPermission(Permissions.TagsDelete);
    }
}

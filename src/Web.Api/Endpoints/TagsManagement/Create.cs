using Application.Abstractions.Messaging;
using Application.Tags.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.TagsManagement;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tags", async (
            Request request,
            ICommandHandler<CreateTagCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTagCommand(request.Name);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.TagsManagement)
        .HasPermission(Permissions.TagsCreate);
    }
}

using Application.Abstractions.Messaging;
using Application.Tags.GetAll;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.TagsManagement;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tags", async (
            IQueryHandler<GetAllTagsQuery, List<TagResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllTagsQuery();

            Result<List<TagResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.TagsManagement)
        .HasPermission(Permissions.TagsRead);
    }
}

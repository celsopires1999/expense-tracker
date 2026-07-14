using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Tags.GetAll;
using SharedKernel;

namespace Expense.Api.Endpoints.TagsManagement;

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

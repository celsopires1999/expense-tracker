using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Categories.GetAll;
using SharedKernel;

namespace Expense.Api.Endpoints.Categories;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", async (
            IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllCategoriesQuery();

            Result<List<CategoryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .HasPermission(Permissions.CategoriesRead);
    }
}

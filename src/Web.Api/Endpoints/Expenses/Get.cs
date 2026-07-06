using Application.Abstractions.Messaging;
using Application.Expenses.Get;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Expenses;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("expenses", async (
            Guid? userId,
            DateOnly? from,
            DateOnly? to,
            Guid? categoryId,
            Guid? tagId,
            IQueryHandler<GetExpensesQuery, List<ExpenseResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetExpensesQuery
            {
                UserId = userId,
                From = from,
                To = to,
                CategoryId = categoryId,
                TagId = tagId
            };

            Result<List<ExpenseResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Expenses)
        .HasPermission(Permissions.ExpensesRead);
    }
}

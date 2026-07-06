using Application.Abstractions.Messaging;
using Application.Expenses.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Expenses;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("expenses/{id:guid}", async (
            Guid id,
            IQueryHandler<GetExpenseByIdQuery, ExpenseResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetExpenseByIdQuery(id);

            Result<ExpenseResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Expenses)
        .HasPermission(Permissions.ExpensesRead);
    }
}

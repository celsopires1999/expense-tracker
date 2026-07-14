using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.PaymentMethods.GetAll;
using SharedKernel;

namespace Expense.Api.Endpoints.PaymentMethods;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payment-methods", async (
            IQueryHandler<GetAllPaymentMethodsQuery, List<PaymentMethodResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllPaymentMethodsQuery();

            Result<List<PaymentMethodResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.PaymentMethods)
        .HasPermission(Permissions.PaymentMethodsRead);
    }
}

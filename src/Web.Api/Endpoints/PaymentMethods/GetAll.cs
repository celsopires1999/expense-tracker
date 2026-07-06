using Application.Abstractions.Messaging;
using Application.PaymentMethods.GetAll;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.PaymentMethods;

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

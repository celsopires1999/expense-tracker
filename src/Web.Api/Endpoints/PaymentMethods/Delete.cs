using Application.Abstractions.Messaging;
using Application.PaymentMethods.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.PaymentMethods;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("payment-methods/{id:guid}", async (
            Guid id,
            ICommandHandler<DeletePaymentMethodCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeletePaymentMethodCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.PaymentMethods)
        .HasPermission(Permissions.PaymentMethodsDelete);
    }
}

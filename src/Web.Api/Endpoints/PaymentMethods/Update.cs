using Application.Abstractions.Messaging;
using Application.PaymentMethods.Update;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.PaymentMethods;

internal sealed class Update : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("payment-methods/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdatePaymentMethodCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePaymentMethodCommand(id, request.Name);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.PaymentMethods)
        .HasPermission(Permissions.PaymentMethodsUpdate);
    }
}

using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.PaymentMethods.Delete;
using SharedKernel;

namespace Expense.Api.Endpoints.PaymentMethods;

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

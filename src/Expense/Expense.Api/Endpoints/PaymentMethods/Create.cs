using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.PaymentMethods.Create;
using SharedKernel;

namespace Expense.Api.Endpoints.PaymentMethods;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("payment-methods", async (
            Request request,
            ICommandHandler<CreatePaymentMethodCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePaymentMethodCommand(request.Name);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.PaymentMethods)
        .HasPermission(Permissions.PaymentMethodsCreate);
    }
}

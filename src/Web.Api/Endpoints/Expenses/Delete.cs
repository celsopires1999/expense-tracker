using Application.Abstractions.Messaging;
using Application.Expenses.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Expenses;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("expenses/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteExpenseCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteExpenseCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Expenses)
        .HasPermission(Permissions.ExpensesDelete);
    }
}

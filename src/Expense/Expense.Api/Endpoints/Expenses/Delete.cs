using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Expenses.Delete;
using SharedKernel;

namespace Expense.Api.Endpoints.Expenses;

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

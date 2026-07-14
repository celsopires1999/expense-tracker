using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Expenses.Update;
using Expense.Domain.Expenses;
using SharedKernel;

namespace Expense.Api.Endpoints.Expenses;

internal sealed class Update : IEndpoint
{
    public sealed class Request
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateOnly Date { get; set; }
        public Guid CategoryId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public List<Guid> TagIds { get; set; } = [];
        public ExpenseStatus? Status { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("expenses/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateExpenseCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateExpenseCommand
            {
                ExpenseId = id,
                Description = request.Description,
                Amount = request.Amount,
                Date = request.Date,
                CategoryId = request.CategoryId,
                PaymentMethodId = request.PaymentMethodId,
                TagIds = request.TagIds,
                Status = request.Status
            };

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Expenses)
        .HasPermission(Permissions.ExpensesUpdate);
    }
}

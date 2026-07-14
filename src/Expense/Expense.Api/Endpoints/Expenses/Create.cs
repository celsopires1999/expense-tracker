using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Authentication;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Expenses.Create;
using SharedKernel;

namespace Expense.Api.Endpoints.Expenses;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateOnly Date { get; set; }
        public Guid CategoryId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public List<Guid> TagIds { get; set; } = [];
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("expenses", async (
            Request request,
            ICommandHandler<CreateExpenseCommand, Guid> handler,
            IUserContext userContext,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateExpenseCommand
            {
                UserId = userContext.UserId,
                Description = request.Description,
                Amount = request.Amount,
                Date = request.Date,
                CategoryId = request.CategoryId,
                PaymentMethodId = request.PaymentMethodId,
                TagIds = request.TagIds
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Expenses)
        .HasPermission(Permissions.ExpensesCreate);
    }
}

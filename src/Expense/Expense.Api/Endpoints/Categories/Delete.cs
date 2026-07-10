using Expense.Application.Abstractions.Messaging;
using Expense.Application.Categories.Delete;
using SharedKernel;
using Expense.Api.Extensions;
using Expense.Api.Infrastructure;

namespace Expense.Api.Endpoints.Categories;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("categories/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteCategoryCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteCategoryCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .HasPermission(Permissions.CategoriesDelete);
    }
}

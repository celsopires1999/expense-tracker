using Expense.Application.Abstractions.Messaging;
using Expense.Application.Categories.Create;
using SharedKernel;
using Expense.Api.Extensions;
using Expense.Api.Infrastructure;

namespace Expense.Api.Endpoints.Categories;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("categories", async (
            Request request,
            ICommandHandler<CreateCategoryCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCategoryCommand(request.Name);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .HasPermission(Permissions.CategoriesCreate);
    }
}

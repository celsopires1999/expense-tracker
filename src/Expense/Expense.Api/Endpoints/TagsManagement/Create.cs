using Expense.Api.Extensions;
using Expense.Api.Infrastructure;
using Expense.Application.Abstractions.Messaging;
using Expense.Application.Tags.Create;
using SharedKernel;

namespace Expense.Api.Endpoints.TagsManagement;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tags", async (
            Request request,
            ICommandHandler<CreateTagCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTagCommand(request.Name);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.TagsManagement)
        .HasPermission(Permissions.TagsCreate);
    }
}

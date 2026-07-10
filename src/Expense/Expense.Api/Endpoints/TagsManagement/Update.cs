using Expense.Application.Abstractions.Messaging;
using Expense.Application.Tags.Update;
using SharedKernel;
using Expense.Api.Extensions;
using Expense.Api.Infrastructure;

namespace Expense.Api.Endpoints.TagsManagement;

internal sealed class Update : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("tags/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateTagCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTagCommand(id, request.Name);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.TagsManagement)
        .HasPermission(Permissions.TagsUpdate);
    }
}

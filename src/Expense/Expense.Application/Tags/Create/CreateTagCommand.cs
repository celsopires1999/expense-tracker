using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Tags.Create;

public sealed record CreateTagCommand(string Name) : ICommand<Guid>;

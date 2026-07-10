using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Tags.Delete;

public sealed record DeleteTagCommand(Guid Id) : ICommand;

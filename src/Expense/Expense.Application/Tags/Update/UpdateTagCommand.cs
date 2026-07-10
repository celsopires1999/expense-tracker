using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Tags.Update;

public sealed record UpdateTagCommand(Guid Id, string Name) : ICommand;

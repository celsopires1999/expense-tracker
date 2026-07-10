using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Categories.Update;

public sealed record UpdateCategoryCommand(Guid Id, string Name) : ICommand;

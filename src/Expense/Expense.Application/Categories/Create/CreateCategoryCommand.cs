using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Categories.Create;

public sealed record CreateCategoryCommand(string Name) : ICommand<Guid>;

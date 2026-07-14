using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;

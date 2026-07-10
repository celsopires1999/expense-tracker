using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Categories.GetAll;

public sealed record GetAllCategoriesQuery : IQuery<List<CategoryResponse>>;

using Application.Abstractions.Messaging;

namespace Application.Categories.GetAll;

public sealed record GetAllCategoriesQuery : IQuery<List<CategoryResponse>>;

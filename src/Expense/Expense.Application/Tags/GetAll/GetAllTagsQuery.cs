using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.Tags.GetAll;

public sealed record GetAllTagsQuery : IQuery<List<TagResponse>>;

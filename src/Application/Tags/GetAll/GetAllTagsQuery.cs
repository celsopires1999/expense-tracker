using Application.Abstractions.Messaging;

namespace Application.Tags.GetAll;

public sealed record GetAllTagsQuery : IQuery<List<TagResponse>>;

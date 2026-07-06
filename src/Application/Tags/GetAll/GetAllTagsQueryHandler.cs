using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tags.GetAll;

internal sealed class GetAllTagsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetAllTagsQuery, List<TagResponse>>
{
    public async Task<Result<List<TagResponse>>> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        List<TagResponse> tags = await context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync(cancellationToken);

        return tags;
    }
}

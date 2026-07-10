using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Categories.GetAll;

internal sealed class GetAllCategoriesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>>
{
    public async Task<Result<List<CategoryResponse>>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        List<CategoryResponse> categories = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);

        return categories;
    }
}

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Expenses.Get;

internal sealed class GetExpensesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetExpensesQuery, List<ExpenseResponse>>
{
    public async Task<Result<List<ExpenseResponse>>> Handle(GetExpensesQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Expenses.Expense> expensesQuery = context.Expenses.AsNoTracking();

        if (query.UserId.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.UserId == query.UserId.Value);
        }

        if (query.From.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.Date >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.Date <= query.To.Value);
        }

        if (query.CategoryId.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.CategoryId == query.CategoryId.Value);
        }

        if (query.TagId.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.Tags.Any(t => t.TagId == query.TagId.Value));
        }

        List<ExpenseResponse> expenses = await expensesQuery
            .Select(e => new ExpenseResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                CategoryId = e.CategoryId,
                PaymentMethodId = e.PaymentMethodId,
                TagIds = e.Tags.Select(t => t.TagId).ToList(),
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return expenses;
    }
}

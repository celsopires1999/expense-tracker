using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Expenses.GetById;

internal sealed class GetExpenseByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetExpenseByIdQuery, ExpenseResponse>
{
    public async Task<Result<ExpenseResponse>> Handle(GetExpenseByIdQuery query, CancellationToken cancellationToken)
    {
        ExpenseResponse? expense = await context.Expenses
            .AsNoTracking()
            .Where(e => e.Id == query.ExpenseId)
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
            .SingleOrDefaultAsync(cancellationToken);

        if (expense is null)
        {
            return Result.Failure<ExpenseResponse>(ExpenseErrors.NotFound(query.ExpenseId));
        }

        return expense;
    }
}

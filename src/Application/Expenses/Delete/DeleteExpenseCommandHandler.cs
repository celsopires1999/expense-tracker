using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Expenses.Delete;

internal sealed class DeleteExpenseCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeleteExpenseCommand>
{
    public async Task<Result> Handle(DeleteExpenseCommand command, CancellationToken cancellationToken)
    {
        Expense? expense = await context.Expenses
            .SingleOrDefaultAsync(e => e.Id == command.ExpenseId, cancellationToken);

        if (expense is null)
        {
            return Result.Failure(ExpenseErrors.NotFound(command.ExpenseId));
        }

        if (expense.UserId != userContext.UserId)
        {
            return Result.Failure(Domain.Users.UserErrors.Unauthorized());
        }

        context.Expenses.Remove(expense);

        expense.Raise(new ExpenseDeletedDomainEvent(expense.Id));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using Expense.Application.Abstractions.Authentication;
using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Expenses.Delete;

internal sealed class DeleteExpenseCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeleteExpenseCommand>
{
    public async Task<Result> Handle(DeleteExpenseCommand command, CancellationToken cancellationToken)
    {
        Expense.Domain.Expenses.Expense? expense = await context.Expenses
            .SingleOrDefaultAsync(e => e.Id == command.ExpenseId, cancellationToken);

        if (expense is null)
        {
            return Result.Failure(ExpenseErrors.NotFound(command.ExpenseId));
        }

        if (expense.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Failure("Expenses.Unauthorized", "You are not authorized to perform this action"));
        }

        context.Expenses.Remove(expense);

        expense.Raise(new ExpenseDeletedDomainEvent(expense.Id));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

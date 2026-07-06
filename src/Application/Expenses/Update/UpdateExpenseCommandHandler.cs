using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Expenses.Update;

internal sealed class UpdateExpenseCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : ICommandHandler<UpdateExpenseCommand>
{
    public async Task<Result> Handle(UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        Expense? expense = await context.Expenses
            .Include(e => e.Tags)
            .SingleOrDefaultAsync(e => e.Id == command.ExpenseId, cancellationToken);

        if (expense is null)
        {
            return Result.Failure(ExpenseErrors.NotFound(command.ExpenseId));
        }

        if (expense.UserId != userContext.UserId)
        {
            return Result.Failure(Domain.Users.UserErrors.Unauthorized());
        }

        bool categoryExists = await context.Categories
            .AnyAsync(c => c.Id == command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure(Error.NotFound(
                "Categories.NotFound",
                $"The category with Id = '{command.CategoryId}' was not found"));
        }

        bool paymentMethodExists = await context.PaymentMethods
            .AnyAsync(p => p.Id == command.PaymentMethodId, cancellationToken);

        if (!paymentMethodExists)
        {
            return Result.Failure(Error.NotFound(
                "PaymentMethods.NotFound",
                $"The payment method with Id = '{command.PaymentMethodId}' was not found"));
        }

        expense.Description = command.Description;
        expense.Amount = command.Amount;
        expense.Date = command.Date;
        expense.CategoryId = command.CategoryId;
        expense.PaymentMethodId = command.PaymentMethodId;
        expense.UpdatedAt = dateTimeProvider.UtcNow;

        expense.Tags.Clear();

        if (command.TagIds.Count != 0)
        {
            expense.Tags = command.TagIds.Select(tagId => new ExpenseTag
            {
                ExpenseId = expense.Id,
                TagId = tagId
            }).ToList();
        }

        expense.Raise(new ExpenseUpdatedDomainEvent(expense.Id));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

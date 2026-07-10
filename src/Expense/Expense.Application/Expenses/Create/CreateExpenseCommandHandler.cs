using Expense.Application.Abstractions.Authentication;
using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.Expenses.Create;

internal sealed class CreateExpenseCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : ICommandHandler<CreateExpenseCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        if (userContext.UserId != command.UserId)
        {
            return Result.Failure<Guid>(Error.Failure("Expenses.Unauthorized", "You are not authorized to perform this action"));
        }

        bool categoryExists = await context.Categories
            .AnyAsync(c => c.Id == command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<Guid>(Error.NotFound(
                "Categories.NotFound",
                $"The category with Id = '{command.CategoryId}' was not found"));
        }

        bool paymentMethodExists = await context.PaymentMethods
            .AnyAsync(p => p.Id == command.PaymentMethodId, cancellationToken);

        if (!paymentMethodExists)
        {
            return Result.Failure<Guid>(Error.NotFound(
                "PaymentMethods.NotFound",
                $"The payment method with Id = '{command.PaymentMethodId}' was not found"));
        }

        var expense = new Expense.Domain.Expenses.Expense
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Description = command.Description,
            Amount = command.Amount,
            Date = command.Date,
            CategoryId = command.CategoryId,
            PaymentMethodId = command.PaymentMethodId,
            CreatedAt = dateTimeProvider.UtcNow
        };

        if (command.TagIds.Count != 0)
        {
            List<Guid> validTagIds = await context.Tags
                .Where(t => command.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            var invalidTags = command.TagIds.Except(validTagIds).ToList();

            if (invalidTags.Count != 0)
            {
                return Result.Failure<Guid>(Error.NotFound(
                    "Tags.NotFound",
                    $"The following tag IDs were not found: {string.Join(", ", invalidTags)}"));
            }

            expense.Tags = validTagIds.Select(tagId => new ExpenseTag
            {
                ExpenseId = expense.Id,
                TagId = tagId
            }).ToList();
        }

        expense.Raise(new ExpenseCreatedDomainEvent(expense.Id));

        context.Expenses.Add(expense);

        await context.SaveChangesAsync(cancellationToken);

        return expense.Id;
    }
}

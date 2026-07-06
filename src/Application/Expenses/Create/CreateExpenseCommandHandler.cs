using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Expenses.Create;

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
            return Result.Failure<Guid>(Domain.Users.UserErrors.Unauthorized());
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

        var expense = new Expense
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

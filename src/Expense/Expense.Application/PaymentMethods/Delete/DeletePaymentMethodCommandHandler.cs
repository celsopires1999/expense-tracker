using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.PaymentMethods.Delete;

internal sealed class DeletePaymentMethodCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeletePaymentMethodCommand>
{
    public async Task<Result> Handle(DeletePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        PaymentMethod? paymentMethod = await context.PaymentMethods
            .SingleOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (paymentMethod is null)
        {
            return Result.Failure(Error.NotFound(
                "PaymentMethods.NotFound",
                $"The payment method with Id = '{command.Id}' was not found"));
        }

        bool inUse = await context.Expenses
            .AnyAsync(e => e.PaymentMethodId == command.Id, cancellationToken);

        if (inUse)
        {
            return Result.Failure(Error.Conflict(
                "PaymentMethods.InUse",
                "The payment method is currently in use by one or more expenses and cannot be deleted"));
        }

        context.PaymentMethods.Remove(paymentMethod);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

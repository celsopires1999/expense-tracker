using Expense.Application.Abstractions.Data;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Expense.Application.PaymentMethods.Update;

internal sealed class UpdatePaymentMethodCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdatePaymentMethodCommand>
{
    public async Task<Result> Handle(UpdatePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        PaymentMethod? paymentMethod = await context.PaymentMethods
            .SingleOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (paymentMethod is null)
        {
            return Result.Failure(Error.NotFound(
                "PaymentMethods.NotFound",
                $"The payment method with Id = '{command.Id}' was not found"));
        }

        bool duplicate = await context.PaymentMethods
            .AnyAsync(p => p.Name == command.Name && p.Id != command.Id, cancellationToken);

        if (duplicate)
        {
            return Result.Failure(Error.Conflict(
                "PaymentMethods.DuplicateName",
                $"A payment method with name '{command.Name}' already exists"));
        }

        paymentMethod.Name = command.Name;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

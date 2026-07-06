using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.PaymentMethods.Create;

internal sealed class CreatePaymentMethodCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreatePaymentMethodCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        bool exists = await context.PaymentMethods
            .AnyAsync(p => p.Name == command.Name, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(Error.Conflict(
                "PaymentMethods.DuplicateName",
                $"A payment method with name '{command.Name}' already exists"));
        }

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = command.Name
        };

        context.PaymentMethods.Add(paymentMethod);

        await context.SaveChangesAsync(cancellationToken);

        return paymentMethod.Id;
    }
}

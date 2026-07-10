using FluentValidation;

namespace Expense.Application.PaymentMethods.Delete;

internal sealed class DeletePaymentMethodCommandValidator : AbstractValidator<DeletePaymentMethodCommand>
{
    public DeletePaymentMethodCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

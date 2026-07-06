using FluentValidation;

namespace Application.Expenses.Update;

internal sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(c => c.ExpenseId).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MinimumLength(3).MaximumLength(255);
        RuleFor(c => c.Amount).GreaterThan(decimal.Zero);
        RuleFor(c => c.Date).NotEmpty();
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.PaymentMethodId).NotEmpty();
    }
}

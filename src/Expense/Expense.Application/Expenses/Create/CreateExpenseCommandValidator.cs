using FluentValidation;

namespace Expense.Application.Expenses.Create;

internal sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MinimumLength(3).MaximumLength(255);
        RuleFor(c => c.Amount).GreaterThan(decimal.Zero);
        RuleFor(c => c.Date).NotEmpty();
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.PaymentMethodId).NotEmpty();
    }
}

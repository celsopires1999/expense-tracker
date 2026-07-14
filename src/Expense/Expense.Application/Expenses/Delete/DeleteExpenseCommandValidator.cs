using FluentValidation;

namespace Expense.Application.Expenses.Delete;

internal sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(c => c.ExpenseId).NotEmpty();
    }
}

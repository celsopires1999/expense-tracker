using SharedKernel;

namespace Domain.Expenses;

public static class ExpenseErrors
{
    public static Error NotFound(Guid expenseId) => Error.NotFound(
        "Expenses.NotFound",
        $"The expense with the Id = '{expenseId}' was not found");
}

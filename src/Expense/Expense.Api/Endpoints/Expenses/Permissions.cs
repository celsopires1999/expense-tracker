namespace Expense.Api.Endpoints.Expenses;

internal static class Permissions
{
    internal const string ExpensesCreate = "expenses:create";
    internal const string ExpensesRead = "expenses:read";
    internal const string ExpensesReadAll = "expenses:read:all";
    internal const string ExpensesUpdate = "expenses:update";
    internal const string ExpensesUpdateAll = "expenses:update:all";
    internal const string ExpensesDelete = "expenses:delete";
    internal const string ExpensesDeleteAll = "expenses:delete:all";
}

using Expense.Domain.Expenses;

namespace Expense.IntegrationTests.Handlers;

public sealed class ExpenseStatusTransitionTests
{
    [Fact]
    public void ChangeStatus_ShouldSucceed_WhenPendingToApproved()
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Pending
        };

        expense.ChangeStatus(ExpenseStatus.Approved);

        Assert.Equal(ExpenseStatus.Approved, expense.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldSucceed_WhenPendingToRejected()
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Pending
        };

        expense.ChangeStatus(ExpenseStatus.Rejected);

        Assert.Equal(ExpenseStatus.Rejected, expense.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldSucceed_WhenApprovedToPaid()
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Approved
        };

        expense.ChangeStatus(ExpenseStatus.Paid);

        Assert.Equal(ExpenseStatus.Paid, expense.Status);
    }

    [Theory]
    [InlineData(ExpenseStatus.Pending)]
    [InlineData(ExpenseStatus.Paid)]
    public void ChangeStatus_ShouldThrow_WhenPendingToInvalid(ExpenseStatus newStatus)
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Pending
        };

        Assert.Throws<InvalidOperationException>(() => expense.ChangeStatus(newStatus));
    }

    [Theory]
    [InlineData(ExpenseStatus.Pending)]
    [InlineData(ExpenseStatus.Rejected)]
    public void ChangeStatus_ShouldThrow_WhenApprovedToInvalid(ExpenseStatus newStatus)
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Approved
        };

        Assert.Throws<InvalidOperationException>(() => expense.ChangeStatus(newStatus));
    }

    [Theory]
    [InlineData(ExpenseStatus.Pending)]
    [InlineData(ExpenseStatus.Approved)]
    [InlineData(ExpenseStatus.Paid)]
    public void ChangeStatus_ShouldThrow_WhenRejectedToInvalid(ExpenseStatus newStatus)
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Rejected
        };

        Assert.Throws<InvalidOperationException>(() => expense.ChangeStatus(newStatus));
    }

    [Theory]
    [InlineData(ExpenseStatus.Pending)]
    [InlineData(ExpenseStatus.Approved)]
    [InlineData(ExpenseStatus.Rejected)]
    public void ChangeStatus_ShouldThrow_WhenPaidToInvalid(ExpenseStatus newStatus)
    {
        var expense = new global::Expense.Domain.Expenses.Expense
        {
            Status = ExpenseStatus.Paid
        };

        Assert.Throws<InvalidOperationException>(() => expense.ChangeStatus(newStatus));
    }
}

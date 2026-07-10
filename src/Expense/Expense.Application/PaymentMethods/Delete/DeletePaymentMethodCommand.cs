using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.PaymentMethods.Delete;

public sealed record DeletePaymentMethodCommand(Guid Id) : ICommand;

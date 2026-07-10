using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.PaymentMethods.Update;

public sealed record UpdatePaymentMethodCommand(Guid Id, string Name) : ICommand;

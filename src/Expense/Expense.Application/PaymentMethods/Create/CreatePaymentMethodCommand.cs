using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.PaymentMethods.Create;

public sealed record CreatePaymentMethodCommand(string Name) : ICommand<Guid>;

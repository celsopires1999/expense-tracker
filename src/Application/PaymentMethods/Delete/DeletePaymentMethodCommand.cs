using Application.Abstractions.Messaging;

namespace Application.PaymentMethods.Delete;

public sealed record DeletePaymentMethodCommand(Guid Id) : ICommand;

using Application.Abstractions.Messaging;

namespace Application.PaymentMethods.Update;

public sealed record UpdatePaymentMethodCommand(Guid Id, string Name) : ICommand;

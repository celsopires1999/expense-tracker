using Application.Abstractions.Messaging;

namespace Application.PaymentMethods.Create;

public sealed record CreatePaymentMethodCommand(string Name) : ICommand<Guid>;

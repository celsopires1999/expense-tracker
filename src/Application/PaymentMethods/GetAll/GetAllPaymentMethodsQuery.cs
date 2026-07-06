using Application.Abstractions.Messaging;

namespace Application.PaymentMethods.GetAll;

public sealed record GetAllPaymentMethodsQuery : IQuery<List<PaymentMethodResponse>>;

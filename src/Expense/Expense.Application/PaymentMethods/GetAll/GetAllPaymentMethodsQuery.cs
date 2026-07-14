using Expense.Application.Abstractions.Messaging;

namespace Expense.Application.PaymentMethods.GetAll;

public sealed record GetAllPaymentMethodsQuery : IQuery<List<PaymentMethodResponse>>;

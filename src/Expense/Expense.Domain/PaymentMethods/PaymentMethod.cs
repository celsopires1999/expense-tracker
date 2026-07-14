using SharedKernel;

namespace Expense.Domain.PaymentMethods;

public sealed class PaymentMethod : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

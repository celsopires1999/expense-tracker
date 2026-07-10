using SharedKernel;

namespace Expense.Domain.Tags;

public sealed class Tag : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

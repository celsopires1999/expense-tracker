using SharedKernel;

namespace Expense.Domain.Categories;

public sealed class Category : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

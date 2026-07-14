using SharedKernel;

namespace Permission.Domain.Roles;

public sealed class Role : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

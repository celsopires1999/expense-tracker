using SharedKernel;

namespace Domain.Roles;

public sealed class UserRole : Entity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

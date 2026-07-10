using SharedKernel;

namespace Permission.Domain.Roles;

public sealed class RolePermission : Entity
{
    public Guid RoleId { get; set; }
    public string Permission { get; set; }
}

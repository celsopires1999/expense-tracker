namespace Permission.Application.Roles.GetAll;

public sealed class RoleDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<string> Permissions { get; set; }
}

namespace Auth.Application.Users.GetAll;

public sealed class ListUsersResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string[] Roles { get; set; }
}

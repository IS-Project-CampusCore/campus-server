namespace users.Model;

public record Communicator(UserType Role) : User(Role)
{
    public required string University { get; set; } = string.Empty;
}
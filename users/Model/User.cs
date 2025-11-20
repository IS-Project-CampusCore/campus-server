namespace users.Model;

public enum UserType { GUEST, ADMIN, MANAGEMENT, STUDENT, PROFESSOR, CAMPUS_STUDENT };

public record User
{
    public required string Id { get; set; } = string.Empty;
    public required string Name { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string PasswordHash { get; set; } = string.Empty;
    public required UserType Role { get; set; } = UserType.GUEST;

    public bool IsVerified = false;

    public User() { }

    public User(UserType type)
    {
        Role = type;
    }
}

public record UserWithJWT(User? User, string JwtToken);

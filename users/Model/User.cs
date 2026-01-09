using commons.Database;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

public enum UserType { GUEST, ADMIN, MANAGEMENT, STUDENT, PROFESSOR, CAMPUS_STUDENT, NO_ROLE };

[CollectionName("Users")]
public record User : DatabaseModel
{
    // Id is inherited from DatabaseModel (ObjectId)

    public required string Name { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public required UserType Role { get; set; } = UserType.GUEST;

    public bool IsVerified { get; set; } = false; // Changed to property for serialization

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public User() { }

    public User(UserType type)
    {
        Role = type;
    }

    public static UserType StringToRole(string role)
    {
        // Your existing static mapper logic remains valid
        string roleLower = role.ToLower();
        if (roleLower == "guest") return UserType.GUEST;
        else if (roleLower == "admin") return UserType.ADMIN;
        else if (roleLower == "management") return UserType.MANAGEMENT;
        else if (roleLower == "student") return UserType.STUDENT;
        else if (roleLower == "professor") return UserType.PROFESSOR;
        else if (roleLower == "campus_student") return UserType.CAMPUS_STUDENT;
        return UserType.NO_ROLE;
    }
}

public record UserWithJwt(User? User, string JwtToken);
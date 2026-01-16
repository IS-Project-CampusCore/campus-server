using commons.Database;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

public enum UserType { GUEST, ADMIN, MANAGEMENT, STUDENT, PROFESSOR, CAMPUS_STUDENT, NO_ROLE };

[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(Communicator))]
[BsonKnownTypes(typeof(Management))]
[BsonKnownTypes(typeof(Admin))]
[CollectionName("Users")]
[BsonIgnoreExtraElements]
public record User : DatabaseModel
{
    public required string Name { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public required UserType Role { get; set; } = UserType.GUEST;

    public bool IsVerified { get; set; } = false; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public User() { }

    public User(UserType type)
    {
        Role = type;
    }

    public static UserType StringToRole(string role)
    {
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
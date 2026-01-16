using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

[BsonKnownTypes(typeof(CampusStudent))]
public record CampusStudent() : Student(UserType.CAMPUS_STUDENT)
{
    public string? Dormitory { get; set; } = string.Empty;
    public string? Room { get; set; } = string.Empty;
}

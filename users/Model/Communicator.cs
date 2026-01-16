using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

[BsonDiscriminator("Communicator")]
[BsonKnownTypes(typeof(Professor))]
[BsonKnownTypes(typeof(Student))]
public record Communicator(UserType Role) : User(Role)
{
    public required string University { get; set; } = string.Empty;
}
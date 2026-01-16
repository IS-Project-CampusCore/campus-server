using MongoDB.Bson.Serialization.Attributes;
using users.Model;

[BsonDiscriminator("Professor")]
public record Professor() : Communicator(UserType.PROFESSOR)
{
    public required string Department { get; set; } = string.Empty;
    public string Title {  get; set; } = string.Empty;
}
    
using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

[BsonDiscriminator("Management")]
public record Management() : User(UserType.MANAGEMENT);
   
using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

[BsonDiscriminator("Admin")]
public record Admin() : User(UserType.ADMIN);

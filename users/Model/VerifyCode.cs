using commons.Database;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace users.Model;

[CollectionName("VerificationCodes")]
public record VerifyCode : DatabaseModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
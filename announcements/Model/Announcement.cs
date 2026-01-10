using commons.Database;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Announcements.Model;

[CollectionName("Announcements")]
public record Announcement : DatabaseModel
{

    public required string Title { get; set; } = string.Empty;
    public required string Message { get; set; } = string.Empty;
    public required string AuthorId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } 

    public Announcement() { }
}
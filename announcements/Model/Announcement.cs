using commons.Database;

namespace Announcements.Model;

[CollectionName("Announcements")]
public record Announcement : DatabaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastEditedAt { get; set; } = DateTime.UtcNow;
}
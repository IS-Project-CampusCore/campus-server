using commons.Database;

namespace Announcements.Model;

[CollectionName("Announcements")]
public record Announcement : DatabaseModel
{
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Author { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastEditedAt { get; set; }
}
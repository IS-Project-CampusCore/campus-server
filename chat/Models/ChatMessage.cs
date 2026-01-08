using commons.Database;

namespace chat.Models;

[CollectionName("Messages")]
public record ChatMessage : DatabaseModel
{
    public string SenderId { get; set; } = string.Empty;
    public string GroupId {  get; set; } = string.Empty;
    public string? Content { get; set; } = string.Empty;
    public List<string>? FilesId { get; set; } = null;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

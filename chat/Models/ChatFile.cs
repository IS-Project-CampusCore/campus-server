using commons.Database;

namespace chat.Models;

[CollectionName("Files")]
public record ChatFile : DatabaseModel
{
    public string FileName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

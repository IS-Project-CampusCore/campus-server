using commons.Database;

namespace chat.Models;

[CollectionName("Groups")]
public record Group : DatabaseModel
{
    public string Name { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public List<string> MembersId { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

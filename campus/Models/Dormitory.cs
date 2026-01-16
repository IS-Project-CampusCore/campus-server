using commons.Database;

namespace campus.Models;

[CollectionName("Dormitories")]
public record Dormitory : DatabaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Amount { get; set; } = 0;
}
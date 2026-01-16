using commons.Database;

namespace campus.Models;

[CollectionName("Issues")]
public record Issue : DatabaseModel
{
    public string IssuerId { get; set; }= string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
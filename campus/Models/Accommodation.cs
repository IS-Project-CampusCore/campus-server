using commons.Database;
using System.Net;

namespace campus.Models;

[CollectionName("Accommodation")]
public record Accommodation : DatabaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

using commons.Database;
using System.Net;

namespace campus.Models;

[CollectionName("Accommodation")]
public record Accommodation : DatabaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Scheddule Timetable { get; set; } = new Scheddule
    {
        OpenTime = 8,   
        CloseTime = 22  
    };

}
public record Scheddule
{
    public int OpenTime { get; set; } = default;
    public int CloseTime { get; set; } = default;
}

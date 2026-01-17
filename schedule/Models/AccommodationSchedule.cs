using commons.Database;

namespace schedule.Models;

[CollectionName("AccommodationSchedule")]
public record AccommodationSchedule : DatabaseModel
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, List<AccScheduleData>> Schedule { get; set; } = [];
}

public record AccScheduleData(int Hour, string Status);

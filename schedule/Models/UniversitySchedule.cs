using commons.Database;

namespace schedule.Models;

[CollectionName("UniversitySchedule")]
public record UniversitySchedule : DatabaseModel
{
    public string University { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public int Year { get; set; } = -1;
    public int Group { get; set; } = -1;
    public string Day {  get; set; } = string.Empty;
    public List<UniScheduleData> Schedule { get; set; } = [];
}

public record UniScheduleData(int Hour, string Course, string Location);

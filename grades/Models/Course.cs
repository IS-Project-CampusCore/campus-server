using commons.Database;

namespace grades.Models;

[CollectionName("Course")]
public record Course : DatabaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string ProfessorId { get; set; } = string.Empty;
    public int Year { get; set; } = -1;
    public List<string> StudentIds { get; set; } = [];
}

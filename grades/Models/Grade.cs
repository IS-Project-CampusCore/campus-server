using commons.Database;

namespace grades.Models;

[CollectionName("Grades")]
public record Grade : DatabaseModel
{
    public string StudentId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public double? Value {  get; set; } = null;
}

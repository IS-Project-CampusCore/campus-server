namespace grades.Models;

public record BulkResult
{
    public int TotalCount { get; set; } = -1;
    public int SuccessCount { get; set; } = -1;
    public int SkipedCount { get; set; } = -1;
    public List<string>? Errors { get; set; } = null;
}

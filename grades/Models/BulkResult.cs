namespace grades.Models;

public record BulkResult<TResult>
{
    public int TotalCount { get; set; } = -1;
    public int SuccessCount { get; set; } = -1;
    public int SkipedCount { get; set; } = -1;
    public TResult Result { get; set; } = default!;
    public List<string>? Errors { get; set; } = null;
}

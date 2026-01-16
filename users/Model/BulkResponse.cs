namespace users.Model;

public record BulkResponse
{
    public int TotalCount { get; set; } = -1;
    public int SuccessCount { get; set; } = -1;
    public int SkipedCount { get; set; } = -1;
    public List<PerUserResult> Results { get; set; } = [];
}

public record PerUserResult(string Email, string? Error = null);

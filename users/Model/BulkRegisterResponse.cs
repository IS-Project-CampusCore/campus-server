namespace users.Model;

public record BulkRegisterResponse
{
    public int TotalCount { get; set; } = -1;
    public int RegisteredCount { get; set; } = -1;
    public int SkipedCount { get; set; } = -1;
    public List<RegisterResult> Results { get; set; } = [];
}

public record RegisterResult(string Email, string? Error = null);

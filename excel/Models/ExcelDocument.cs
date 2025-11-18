namespace excel.Models;

public record ExcelDocument
{
    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public string Hash { get; set; } = default!;
}

using commons.Database;

namespace excel.Models;

[CollectionName("ExcelDocuments")]
public record ExcelDocument : DatabaseModel
{
    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public string Hash { get; set; } = default!;
}

namespace users.Model;
public class ExcelData
{
    public List<string> Headers { get; set; } = new();
    public List<List<ExcelCell?>> Rows { get; set; } = new();
}
public record ExcelCell(string CellType, object? Value);

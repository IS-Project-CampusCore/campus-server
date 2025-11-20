namespace excel.Models;
public class ExcelData
{
    public List<string> Headers { get; set; } = new();
    public List<List<string?>> Rows { get; set; } = new();
}


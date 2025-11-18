using ClosedXML.Excel;
using excel.Models;

namespace excel.Utils;

public static class ExcelParser
{
    public static ParsedExcel ParseGeneric(string filePath)
    {
        var result = new ParsedExcel();

        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);

        var headerRow = ws.Row(1);
        var lastHeaderCell = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

        for (int col = 1; col <= lastHeaderCell; col++)
        {
            var headerText = headerRow.Cell(col).GetString().Trim();
            result.Headers.Add(headerText);
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = 2; row <= lastRow; row++)
        {
            var excelRow = ws.Row(row);
            var rowValues = new List<string>();

            for (int col = 1; col <= lastHeaderCell; col++)
            {
                var cellVaue = excelRow.Cell(col).GetString().Trim();
                rowValues.Add(cellVaue);
            }

            // ingnore completly empty rows
            if (rowValues.Any(v => !string.IsNullOrEmpty(v)))
                result.Rows.Add(rowValues);
        }

        return result;
    }
}

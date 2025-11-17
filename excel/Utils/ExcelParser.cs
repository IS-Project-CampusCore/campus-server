using ClosedXML.Excel;
using excel.Models;

namespace excel.Utils;

public static class ExcelParser
{
    public enum ExcelTemplateType
    {
        Unknown,
        Grades, 
        Users
    }

    // Ce tip de fisier a fost introdus si comform carui mod trebe parsat
    public static ExcelTemplateType DetectTemplate(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);

        var headerCells = ws.Row(1).Cells().Select(c => c.GetString().Trim()).ToList();

        //Grades template
        if (headerCells.Contains("StudentId") && headerCells.Contains("FirstName") && headerCells.Contains("CourseId"))
        {
            return ExcelTemplateType.Grades;
        }

        //Users template
        if (headerCells.Contains("Email") && headerCells.Contains("Role"))
        {
            return ExcelTemplateType.Users;
        }

        return ExcelTemplateType.Unknown;
    }

    public static List<GradeRow> ParseGrades(string filePath)
    {
        var result = new List<GradeRow>();
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);
        var lastRow = ws.LastRowUsed().RowNumber();

        for (int row = 2; row < lastRow; row++)
        {
            var r = ws.Row(row);
            result.Add(new GradeRow
            {
                StudentId = r.Cell(2).GetString(),
                FirstName = r.Cell(3).GetString(),
                LastName = r.Cell(4).GetString(),
                CourseId = r.Cell(5).GetString(),
                Grade = r.Cell(6).GetString()
            });
        }
        return result;
    }

    public static List<UserRow> ParseUsers(string filePath)
    {
        var result = new List<UserRow>();

        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);
        var lastRow = ws.LastRowUsed().RowNumber();

        for (int row = 2;row < lastRow; row++)
        {
            var r = ws.Row(row);
            result.Add(new UserRow
            {
                // presupunem ca in celula 1 sunt niste numere care doar sunt?
                FirstName = r.Cell(2).GetString(),
                LastName = r.Cell(3).GetString(),
                Email = r.Cell(4).GetString(),
                Role = r.Cell(5).GetString(),
            });
        }
        return result;
    }
}
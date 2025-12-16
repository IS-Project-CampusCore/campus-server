using ClosedXML.Excel;
using commons;
using excel.Models;

namespace excel.Implementation;

public class ExcelServiceImplementation(
    ILogger<ExcelServiceImplementation> logger,
    IConfiguration config)
{
    private readonly ILogger<ExcelServiceImplementation> _logger = logger;

    private string _basePath => config["StorageDir"] ?? "FileStorage/ExcelFiles";
    private readonly Dictionary<string, ExcelDocument> _store = [];

    public async Task<ExcelDocument> UpsertAsync(string fileName, byte[] content) => _store.TryGetValue(fileName, out _) ? await UpdateAsync(fileName, content) : await InsertAsync(fileName, content);

    public async Task<ExcelDocument> InsertAsync(string fileName, byte[] content)
    {
        if (_store.TryGetValue(fileName, out _))
        {
            throw new InternalErrorException("File already exists");
        }

        var filePath = Path.Combine(_basePath, fileName);

        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, content);

        string hash = ComputeHash(content);

        var newDoc = new ExcelDocument
        {
            FileName = fileName,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow,
            Hash = hash,
        };
        _store[fileName] = newDoc;

        return newDoc;
    }

    public async Task<ExcelDocument> UpdateAsync(string fileName, byte[] content)
    {
        if (!_store.TryGetValue(fileName, out var existingDoc) || existingDoc is null)
        {
            throw new NotFoundException($"File:{fileName} does not exist");
        }

        var newHash = ComputeHash(content);
        if (existingDoc.Hash == newHash)
        {
            _logger.LogInformation("No changes made to the file");
            return existingDoc;
        }

        var filePath = existingDoc.FilePath;

        await File.WriteAllBytesAsync(filePath, content);

        ExcelDocument updatedDoc = new ExcelDocument
        {
            FileName = fileName,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow,
            Hash = newHash,
        };

        _store[fileName] = updatedDoc;

        return updatedDoc;
    }

    public ExcelData ParseExcelFile(string fileName)
    {
        var result = new ExcelData();

        if (!_store.ContainsKey(fileName))
        {
            throw new NotFoundException($"File {fileName} is not in database");
        }

        string filePath = Path.Combine(_basePath, fileName);

        try
        {
            using var workbook = new XLWorkbook(filePath);
            if (workbook.Worksheets.Count <= 0)
            {
                throw new BadRequestException($"Excel file at:{filePath} is empty");
            }

            var ws = workbook.Worksheets.First();

            var headerRow = ws.Row(1);
            if (headerRow is null)
            {
                throw new BadRequestException($"Excel file at:{filePath} has no headers");
            }

            int lastHeaderCell = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
            if (lastHeaderCell <= 0 || headerRow.IsEmpty())
            {
                throw new BadRequestException($"Excel file at:{filePath} has no header cells");
            }

            List<string> headers = [];
            foreach (var cell in headerRow.Cells(1, lastHeaderCell))
            {
                string headerText = cell.GetString().Trim();
                headers.Add(headerText);
            }

            result.Headers = headers;

            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow <= 1)
            {
                throw new BadRequestException($"Excel file at:{filePath} has no data cells");
            }

            foreach (var row in ws.Rows(2, lastRow))
            {
                List<string?> rowValues = new List<string?>();
                foreach (var cell in row.Cells(1, lastHeaderCell))
                {
                    if (cell.TryGetValue<string>(out string cellValue))
                    {
                        rowValues.Add(cellValue);
                    }
                    else
                    {
                        //for future implementation this should store those empty cells and tell the user about them
                        _logger.LogWarning($"Data missing from the cell:({cell.Address.RowNumber}, {cell.Address.ColumnNumber})");
                        rowValues.Add(null);
                    }
                }
                result.Rows.Add(rowValues);
            }
        }
        catch (NotFoundException)
        {
            throw new NotFoundException($"No Excel file found at:{filePath}");
        }
        catch (IOException)
        {
            throw new InternalErrorException($"Could not access Excel file at:{filePath}");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }

        return result;
    }

    private static string ComputeHash(byte[] content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha.ComputeHash(content);
        return Convert.ToBase64String(hashBytes);
    }
}
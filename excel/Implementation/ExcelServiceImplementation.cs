using ClosedXML.Excel;
using commons.Database;
using commons.RequestBase;
using commons.Tools;
using excel.Models;
using MongoDB.Driver;

namespace excel.Implementation;

public class ExcelServiceImplementation(
    ILogger<ExcelServiceImplementation> logger,
    IDatabase database,
    IConfiguration config)
{
    private readonly ILogger<ExcelServiceImplementation> _logger = logger;
    private readonly AsyncLazy<IDatabaseCollection<ExcelDocument>> _documents = new(() => GetExcelCollection(database));

    private string _basePath => config["StorageDir"] ?? "FileStorage/ExcelFiles";

    public async Task<ExcelDocument> UpsertAsync(string fileName, byte[] content)
    {
        var documents = await _documents;

        ExcelDocument document;
        if (await documents.ExistsAsync(ed => ed.FileName == fileName))
        {
            document = await UpdateAsync(fileName, content);
        }
        else
        {
            document = await InsertAsync(fileName, content);
        }

        return document;
    }

    public async Task<ExcelDocument> InsertAsync(string fileName, byte[] content)
    {
        var documents = await _documents;

        if (await documents.ExistsAsync(ed => ed.FileName == fileName))
        {
            throw new BadRequestException($"File {fileName} already exists");
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
            LastModifiedAt = DateTime.UtcNow,
            Hash = hash,
        };

        await documents.InsertAsync(newDoc);
        return newDoc;
    }

    public async Task<ExcelDocument> UpdateAsync(string fileName, byte[] content)
    {
        var documents = await _documents;

        ExcelDocument existingDoc = await documents.GetOneAsync(ed => ed.FileName == fileName);
        if (existingDoc is null)
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
            Id = existingDoc.Id,
            FileName = fileName,
            FilePath = filePath,
            UploadedAt = existingDoc.UploadedAt,
            LastModifiedAt = DateTime.UtcNow,
            Hash = newHash,
        };

        await documents.ReplaceAsync(updatedDoc);
        return updatedDoc;
    }

    public async Task<ExcelData> ParseExcelFile(string fileName,List<string> types)
    {
        var result = new ExcelData();

        var documents = await _documents;
        var document = await documents.GetOneAsync(ed => ed.FileName == fileName);

        if (document is null)
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
                headers.Add(cell.GetString().Trim());
            }

            result.Headers = headers;

            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow <= 1)
            {
                throw new BadRequestException($"Excel file at:{filePath} has no data cells");
            }

            foreach (var row in ws.Rows(2, lastRow))
            {
                List<ExcelCell?> rowValues = [];

                foreach (var cell in row.Cells(1, lastHeaderCell))
                {
                    IXLCell sourceCell = cell.IsMerged() ? cell.MergedRange().FirstCell() : cell;

                    ExcelCell? excelCell = null;
                    if (sourceCell.Value.IsBlank)
                    {
                        if (types[sourceCell.Address.ColumnNumber - 1].Contains("?"))
                        {
                            rowValues.Add(null);
                            continue;
                        }
                        result.Errors.Add($"[Excel Error] Empty cell at Row:{sourceCell.Address.RowNumber}, Column:{sourceCell.Address.ColumnNumber}");
                    }
                    else if (sourceCell.Value.IsNumber)
                    {
                        if (sourceCell.TryGetValue<double>(out var value))
                        {
                            excelCell = new ExcelCell("double", value);
                        }
                    }
                    else if (sourceCell.Value.IsBoolean)
                    {
                        if (sourceCell.TryGetValue<bool>(out var value))
                        {
                            excelCell = new ExcelCell("bool", value);
                        }
                    }
                    else if (sourceCell.Value.IsDateTime)
                    {
                        if (sourceCell.TryGetValue<DateTime>(out var value))
                        {
                            excelCell = new ExcelCell("DateTime", value);
                        }
                    }
                    else if (sourceCell.Value.IsText)
                    {
                        if (sourceCell.TryGetValue<string>(out var value))
                        {
                            excelCell = new ExcelCell("string", value);
                        }
                    }
                    else if (sourceCell.Value.IsError)
                    {
                        if (cell.TryGetValue<XLError>(out var value))
                        {
                            excelCell = new ExcelCell("Error", value);
                        }
                    }
                    if (excelCell is not null)
                    {
                        if (excelCell.CellType != types[sourceCell.Address.ColumnNumber - 1] && $"{excelCell.CellType}?" != types[sourceCell.Address.ColumnNumber - 1])
                        {
                            result.Errors.Add($"[Excel Error] Type mismatch at Row:{sourceCell.Address.RowNumber}, Column:{sourceCell.Address.ColumnNumber}. Expected:{types[sourceCell.Address.ColumnNumber - 1]}, Found:{excelCell.CellType}");
                        }
                        rowValues.Add(excelCell);
                    }
                    else
                    {
                        result.Errors.Add($"[Excel Error] Unsupported cell type at Row:{sourceCell.Address.RowNumber}, Column:{sourceCell.Address.ColumnNumber}");
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

    internal static async Task<IDatabaseCollection<ExcelDocument>> GetExcelCollection(IDatabase database)
    {
        var collection = database.GetCollection<ExcelDocument>();

        var nameIndex = Builders<ExcelDocument>.IndexKeys.Ascending(ed => ed.FileName);
        var hashIndex = Builders<ExcelDocument>.IndexKeys.Ascending(ed => ed.Hash);

        CreateIndexModel<ExcelDocument> index1 = new(nameIndex, new CreateIndexOptions
        {
            Name = "excelNameIndex"
        });

        CreateIndexModel<ExcelDocument> index2 = new(hashIndex, new CreateIndexOptions
        {
            Name = "excelHashIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync([index1, index2]);
        return collection;
    }
}
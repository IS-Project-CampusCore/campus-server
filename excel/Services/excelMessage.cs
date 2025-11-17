using Grpc.Core;
using excelServiceClient;
using commons;
using commons.Protos;
using excel.Models;
using excel.Utils;

namespace excel.Services;

public class ExcelDocument
{
    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public string Hash { get; set; } = default!;
}

public class excelMessage : excelService.excelServiceBase
{
    //temporary MongoDb storage
    private static readonly Dictionary<string, ExcelDocument> _store = new();

    private readonly string _basePath;

    public excelMessage(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(env.ContentRootPath, "storage", "excel");
        Directory.CreateDirectory(_basePath);
    }

    private static string ComputeHash(byte[] content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(content);
        return Convert.ToBase64String(hash);
    }

    // daca vrem sa  dam iar parse la ceva care e stocat
    public override async Task<MessageResponse> ParseExcel(ParseExcelRequest request, ServerCallContext context)
    {
        if (!_store.TryGetValue(request.FileName, out var doc))
            return MessageResponse.NotFound($"File '{request.FileName}' nu a fost gasita.");

        var template = ExcelParser.DetectTemplate(doc.FileName);

        switch(template)
        {
            case ExcelParser.ExcelTemplateType.Grades:
            {
                var rows = ExcelParser.ParseGrades(doc.FilePath);
                return MessageResponse.Ok(new
                {
                    template = "Grades",
                    fileName = doc.FileName,
                    uploadedAt = doc.UploadedAt,
                    rowCount = rows.Count,
                    rows
                });

            }

            case ExcelParser.ExcelTemplateType.Users:
            {
                var rows = ExcelParser.ParseUsers(doc.FilePath);
                return MessageResponse.Ok(new
                {
                    template = "Users",
                    fileName = doc.FileName,
                    uploadedAt = doc.UploadedAt,
                    rowCount = rows.Count,
                    rows
                });

            }

            default:
                return MessageResponse.BadRequest("Unknown Excel template.");
        }
    }

    // Main pipeline:
    // - folosita mereu cand un excel e incarcat
    // - decide daca e new upload sau update
    // - face parsarea in concordanta
    public override async Task<MessageResponse> CheckOrUpdate(CheckOrUpdateExcelRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || request.Content.Length == 0)
            return MessageResponse.BadRequest("File name sau continutul lipseste.");

        var fileName = request.FileName.Trim();
        var newBytes = request.Content.ToByteArray();
        var newHash = ComputeHash(newBytes);

        var filePath = Path.Combine(_basePath, fileName);

        if (_store.TryGetValue(fileName, out var existing))
        {
            if (existing.Hash == newHash)
            {
                return MessageResponse.Ok(new
                {
                    action = "NoChange",
                    fileName,
                    message = "Excel-ul exista deja si este identic"
                } );
            }

            var oldFilePath = existing.FilePath;
            var oldTemplate = ExcelParser.DetectTemplate(oldFilePath);

            object? oldRows = null; // nu stim daca is note sau users
            object? newRows = null;
            string templateName;
            
            switch(oldTemplate)
            {
                case ExcelParser.ExcelTemplateType.Grades:
                    oldRows = ExcelParser.ParseGrades(oldFilePath);
                    templateName = "Grades";
                    break;
                case ExcelParser.ExcelTemplateType.Users:
                    oldRows = ExcelParser.ParseUsers(oldFilePath);
                    templateName = "Users";
                    break;

                default:
                    return MessageResponse.BadRequest("Unknown excel template pentru un fisier care deja exista.");
            }

            //Overwrite existing file
            await File.WriteAllBytesAsync(oldFilePath, newBytes, context.CancellationToken);
            existing.Hash = newHash;
            existing.UploadedAt = DateTime.UtcNow;

            // Parsare iar dupa ca a fost supra suprascris fisierul (are acelasi path)
            switch(oldTemplate)
            {
                case ExcelParser.ExcelTemplateType.Grades:
                    newRows = ExcelParser.ParseGrades(oldFilePath);
                    break;
                case ExcelParser .ExcelTemplateType.Users:
                    newRows = ExcelParser.ParseUsers (oldFilePath);
                    break;
            }

            //Momentan doar trimitem randurile, si cele vechi si cele noi
            // putem face dupa partea de a vedea diferentele intre randuri
            return MessageResponse.Ok(new
            {
                action = "Updated",
                template = templateName,
                fileName,
                updatedAt = existing.UploadedAt,
                oldRows,
                newRows
            });
        }
        else 
        {
            //New File
            await File.WriteAllBytesAsync(filePath, newBytes, context.CancellationToken);

            var doc = new ExcelDocument
            {
                FileName = fileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                Hash = newHash
            };
            _store[fileName] = doc;

            var template = ExcelParser.DetectTemplate(filePath);

            object? rows;
            string templateName;

            switch(template)
            {
                case ExcelParser.ExcelTemplateType.Grades:
                    rows = ExcelParser.ParseGrades(filePath);
                    templateName = "Grades";
                    break;
                case ExcelParser.ExcelTemplateType.Users:
                    rows = ExcelParser.ParseUsers(filePath);
                    templateName = "Users";
                    break;
                default:
                    return MessageResponse.BadRequest("Unknown Excel Template");
            }

            return MessageResponse.Ok(new
            {
                action = "Created",
                template = templateName,
                fileName,
                storedPath = filePath,
                updatedAt = DateTime.UtcNow,
                rows
            });
        }
    }
}
using commons.Protos;
using excel.Models;
using excel.Utils;
using excelServiceClient;
using System.Diagnostics.Contracts;

namespace excel.Implementation;

public class CheckOrUpdateMessage
{
    private readonly string _basePath;
    private readonly Dictionary<string, ExcelDocument> _store;

    public CheckOrUpdateMessage(string basePath, Dictionary<string, ExcelDocument?> store)
    {
        _basePath = basePath; _store = store;
        _store = store;
    }

    public MessageResponse Execute(CheckOrUpdateExcelRequest request)
    {
        if (string.IsNullOrEmpty(request.FileName) || request.Content.Length == 0)
            return MessageResponse.BadRequest("File name or content is missing.");

        var fileName = request.FileName.Trim();
        var newBytes = request.Content.ToByteArray();
        var newHash = ComputeHash(newBytes);

        var filePath = Path.Combine(_basePath, fileName);

        // 1. File already exists 
        if (_store.TryGetValue(filePath, out var existingDoc))
        {
            if (existingDoc.Hash == newHash)
            {
                return MessageResponse.Ok(new
                {
                    action = "NoChange",
                    fileName,
                    MessageResponse = "File already exits and it is identical."
                });
            }
                var oldFilePath = existingDoc.FilePath;
                var oldParsed = ExcelParser.ParseGeneric(oldFilePath);

            File.WriteAllBytes(oldFilePath, newBytes);
            existingDoc.Hash = newHash;
            existingDoc.UploadedAt = DateTime.UtcNow;

            var newParsed = ExcelParser.ParseGeneric(oldFilePath);

            return MessageResponse.Ok(new
            {
                action = "Updated",
                fileName,
                updatedAt = existingDoc.UploadedAt,
                oldHeaders = oldParsed.Headers,
                oldRows = oldParsed.Rows,
                newHeaders = newParsed.Headers,
                newRows = newParsed.Rows,
                message = "File updated."
            });
            

        }
        // 2. Totally new file

        File.WriteAllBytes(filePath, newBytes);

        var newDoc = new ExcelDocument
        {
            FileName = fileName,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow,
            Hash = newHash,
        };
        _store[fileName] = newDoc;

        // Parse new file
        var parsedExcel = ExcelParser.ParseGeneric(filePath);

        return MessageResponse.Ok(new
        {
            action = "Created",
            fileName,
            storedPath = filePath,
            uploadedAt = newDoc.UploadedAt,
            headers = parsedExcel.Headers,
            rows = parsedExcel.Rows,
        });
    }

    private static string ComputeHash(byte[] content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha.ComputeHash(content);
        return Convert.ToBase64String(hashBytes);
    }
}
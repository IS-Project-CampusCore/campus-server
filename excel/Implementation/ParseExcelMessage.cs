using commons.Protos;
using excel.Models;
using excel.Utils;
using excelServiceClient;

namespace excel.Implementation;

public class ParseExcelMessage
{
    private readonly string _basePath;
    private readonly Dictionary<string, ExcelDocument> _store;

    public ParseExcelMessage(string basePath, Dictionary<string, ExcelDocument> store)
    {
        _basePath = basePath;
        _store = store;
    }

    public MessageResponse Excecute(ParseExcelRequest request)
    {
        if (!_store.TryGetValue(request.FileName, out var doc))
            return MessageResponse.NotFound($"FIle '{request.FileName}' not found.");

        var parsed = ExcelParser.ParseGeneric(doc.FilePath);

        return MessageResponse.Ok(new
        {
            headers = parsed.Headers,
            rows = parsed.Rows,
            uploadedAt = doc.UploadedAt
        });
    }
}
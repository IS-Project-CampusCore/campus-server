using Grpc.Core;
using excelServiceClient;
using commons;
using commons.Protos;
using excel.Implementation;
using excel.Models;

namespace excel.Services;

public class excelMessage : excelService.excelServiceBase
{
    // mock MongoDb store

    private static readonly Dictionary<string, ExcelDocument> _store = new();

    private readonly ParseExcelMessage _parse;
    private readonly CheckOrUpdateMessage _checkOrUpdate;

    public excelMessage(IWebHostEnvironment env)
    {
        var basePath = Path.Combine(env.ContentRootPath, "storage", "excel");
        Directory.CreateDirectory(basePath);

        _parse = new ParseExcelMessage(basePath, _store);
        _checkOrUpdate = new CheckOrUpdateMessage(basePath, _store);
    }

    public override Task<MessageResponse> ParseExcel(ParseExcelRequest request, ServerCallContext context)
        => Task.FromResult(_parse.Excecute(request));

    public override Task<MessageResponse> CheckOrUpdate(CheckOrUpdateExcelRequest request, ServerCallContext context)
        => Task.FromResult(_checkOrUpdate.Execute(request));
}
using commons.RequestBase;
using excel.Implementation;
using excel.Models;
using excelServiceClient;

namespace excel.Services;

public class InsertExcelMessage(
    ILogger<InsertExcelMessage> logger,
    ExcelServiceImplementation implementation
) : CampusMessage<InsertExcelRequest, ExcelDocument>(logger)
{
    private readonly ExcelServiceImplementation _impl = implementation;

    protected override async Task<ExcelDocument> HandleMessage(InsertExcelRequest request, CancellationToken token)
        => await _impl.InsertAsync(request.FileName, request.Content.ToArray());
}
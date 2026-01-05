using commons.RequestBase;
using excel.Implementation;
using excel.Models;
using excelServiceClient;

namespace excel.Services;

public class UpsertExcelMessage(
    ILogger<UpsertExcelMessage> logger,
    ExcelServiceImplementation implementation
) : CampusMessage<UpsertExcelRequest, ExcelDocument>(logger)
{
    private readonly ExcelServiceImplementation _impl = implementation;

    protected override async Task<ExcelDocument> HandleMessage(UpsertExcelRequest request, CancellationToken cancellationToken) =>
        await _impl.UpsertAsync(request.FileName, request.Content.ToArray());
}
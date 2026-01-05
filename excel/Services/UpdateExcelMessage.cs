using commons.RequestBase;
using excel.Implementation;
using excel.Models;
using excelServiceClient;

namespace excel.Services;

public class UpdateExcelMessage(
    ILogger<UpdateExcelMessage> logger,
    ExcelServiceImplementation implementation
) : CampusMessage<UpdateExcelRequest, ExcelDocument>(logger)
{
    private readonly ExcelServiceImplementation _impl = implementation;

    protected override async Task<ExcelDocument> HandleMessage(UpdateExcelRequest request, CancellationToken cancellationToken) =>
            await _impl.UpdateAsync(request.FileName, request.Content.ToArray());
}
using commons.RequestBase;
using excel.Implementation;
using excel.Models;
using excelServiceClient;

namespace excel.Services;

public class ParseExcelMessage(
    ILogger<ParseExcelMessage> logger,
    ExcelServiceImplementation implementation
) : CampusMessage <ParseExcelRequest, ExcelData>(logger)
{
    private readonly ExcelServiceImplementation _impl = implementation;

    protected override async Task<ExcelData> HandleMessage(ParseExcelRequest request, CancellationToken token) 
        => await _impl.ParseExcelFile(request.FileName);
}
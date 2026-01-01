using commons;
using commons.Protos;
using excel.Implementation;
using excel.Models;
using excelServiceClient;
using Grpc.Core;
using MediatR;

namespace excel.Services;

public class ParseExcelMessage(
    ILogger<ParseExcelMessage> logger,
    ExcelServiceImplementation implementation
) : IRequestHandler<ParseExcelRequest, MessageResponse>
{
    private readonly ExcelServiceImplementation _impl = implementation;
    private readonly ILogger<ParseExcelMessage> _logger = logger;

    public async Task<MessageResponse> Handle(ParseExcelRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.FileName))
        {
            _logger.LogError("ParseExcel Request is empty");
            return MessageResponse.BadRequest("ParseExcel Request is empty");
        }

        try
        {
            ExcelData excelData = await _impl.ParseExcelFile(request.FileName);

            return MessageResponse.Ok(excelData);
        }
        catch (Exception ex)
        {
            return MessageResponse.Error(ex);
        }
    }
}
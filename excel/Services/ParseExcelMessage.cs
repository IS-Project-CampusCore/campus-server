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

    public Task<MessageResponse> Handle(ParseExcelRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.FileName))
        {
            _logger.LogError("ParseExcel Request is empty");
            return Task.FromResult(MessageResponse.BadRequest("ParseExcel Request is empty"));
        }

        try
        {
            ExcelData excelData = _impl.ParseExcelFile(request.FileName);

            return Task.FromResult(MessageResponse.Ok(excelData));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MessageResponse.Error(ex));
        }
    }
}
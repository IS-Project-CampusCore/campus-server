using commons;
using commons.Protos;
using excel.Implementation;
using excel.Models;
using excelServiceClient;
using MediatR;

namespace excel.Services;

public class InsertExcelMessage(
    ILogger<InsertExcelMessage> logger,
    ExcelServiceImplementation implementation
) : IRequestHandler<InsertExcelRequest, MessageResponse>
{
    private readonly ExcelServiceImplementation _impl = implementation;
    private readonly ILogger<InsertExcelMessage> _logger = logger;

    public async Task<MessageResponse> Handle(InsertExcelRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.FileName) || request.Content is null ||request.Content.IsEmpty)
        {
            _logger.LogError("Excel Request is empty");
            return MessageResponse.BadRequest("Excel Request is empty");
        }

        try
        {
            ExcelDocument excelData = await _impl.InsertAsync(request.FileName, request.Content.ToArray());

            return MessageResponse.Ok(excelData);
        }
        catch (Exception ex)
        {
            return MessageResponse.Error(ex);
        }
    }
}
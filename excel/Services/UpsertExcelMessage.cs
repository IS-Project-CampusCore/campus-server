using commons;
using commons.Protos;
using excel.Implementation;
using excel.Models;
using excelServiceClient;
using MediatR;

namespace excel.Services;

public class UpsertExcelMessage(
    ILogger<UpsertExcelMessage> logger,
    ExcelServiceImplementation implementation
) : IRequestHandler<UpsertExcelRequest, MessageResponse>
{
    private readonly ExcelServiceImplementation _impl = implementation;
    private readonly ILogger<UpsertExcelMessage> _logger = logger;

    public async Task<MessageResponse> Handle(UpsertExcelRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.FileName) || request.Content is null || request.Content.IsEmpty)
        {
            _logger.LogError("Excel Request is empty");
            return MessageResponse.BadRequest("Excel Request is empty");
        }

        try
        {
            ExcelDocument excelData = await _impl.UpsertAsync(request.FileName, request.Content.ToArray());

            return MessageResponse.Ok(excelData);
        }
        catch (Exception ex)
        {
            return MessageResponse.Error(ex);
        }
    }
}
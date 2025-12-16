using commons.Protos;
using excelServiceClient;
using Grpc.Core;
using MediatR;

namespace excel;

public class ExcelService(IMediator mediator) : excelService.excelServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> InsertExcel(InsertExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> UpdateExcel(UpdateExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> UpsertExcel(UpsertExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> ParseExcel(ParseExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}

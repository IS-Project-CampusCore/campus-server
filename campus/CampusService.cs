using commons.Protos;
using campusServiceClient;
using Grpc.Core;
using MediatR;

namespace campus;

public class CampusService(IMediator mediator) : campusService.campusServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> CreateAccommodation(CreateAccommodationRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GenerateDistribution(GenerateDistributionRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> CreatePayment(CreatePaymentRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> ReportIssue(ReportIssueRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetAccommodationById(GetAccByIdRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetAccommodations(GetAccsRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}


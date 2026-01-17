using commons.Protos;
using scheduleServiceClient;
using Grpc.Core;
using MediatR;

namespace schedule;

public class ScheduleService(IMediator mediator) : scheduleService.scheduleServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> UpsertUniversitySchedule(UpsertUniScheduleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> UpsertAccommodationSchedule(UpsertAccScheduleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetUniversitySchedule(GetUniScheduleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetAccommodationSchedule(GetAccScheduleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}


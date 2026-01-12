using announcementsServiceClient;
using commons.Protos;
using Grpc.Core;
using MediatR;

namespace announcements;

public class AnnouncementsService(IMediator mediator) : announcementsService.announcementsServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> CreateAnnouncement(CreateAnnouncementRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> EditAnnouncement(EditAnnouncementRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> DeleteAnnouncement(DeleteAnnouncementRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}
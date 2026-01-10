using commons.Protos;
using announcementsServiceClient;
using Grpc.Core;
using MediatR;

namespace Announcements;

public class AnnouncementsService(IMediator mediator) : announcementsService.announcementsServiceBase
{
    private readonly IMediator _mediator = mediator;

    /*public override async Task<MessageResponse> ExampleMessage(ExampleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }*/
}


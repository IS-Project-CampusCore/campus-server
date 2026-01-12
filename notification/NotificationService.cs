using commons.Protos;
using notificationServiceClient;
using Grpc.Core;
using MediatR;

namespace Notification;

public class NotificationService(IMediator mediator) : notificationService.notificationServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> ExampleMessage(ExampleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}


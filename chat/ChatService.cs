using commons.Protos;
using chatServiceClient;
using Grpc.Core;
using MediatR;

namespace Chat;

public class ChatService(IMediator mediator) : chatService.chatServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}


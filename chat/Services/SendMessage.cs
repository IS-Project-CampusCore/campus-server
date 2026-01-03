using Grpc.Core;
using commons;
using commons.Protos;
using chatServiceClient;
using MediatR;
using Chat.Implementation;

namespace Chat.Services;

public class SendMessage(
    ILogger<SendMessage> logger,
    ChatServiceImplementation implementation
) : IRequestHandler<SendMessageRequest, MessageResponse>
{
    private readonly ChatServiceImplementation _impl = implementation;
    private readonly ILogger<SendMessage> _logger = logger;

    public Task<MessageResponse> Handle(SendMessageRequest request, CancellationToken token)
    {
        return Task.FromResult(MessageResponse.Ok());
    }
}

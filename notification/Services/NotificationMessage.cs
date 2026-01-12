using Grpc.Core;
using commons;
using commons.Protos;
using notificationServiceClient;
using MediatR;
using Notification.Implementation;

namespace Notification.Services;

public class ExampleMessage(
    ILogger<ExampleMessage> logger,
    ChatNotificationImplementation implementation
) : IRequestHandler<ExampleRequest, MessageResponse>
{
    private readonly ChatNotificationImplementation _impl = implementation;
    private readonly ILogger<ExampleMessage> _logger = logger;

    public Task<MessageResponse> Handle(ExampleRequest request, CancellationToken token)
    {
        return Task.FromResult(MessageResponse.Ok());
    }
}

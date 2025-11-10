using Grpc.Core;
using usersServiceClient;
using commons;
using commons.Protos;

namespace users.Services;

public class usersMessage : usersService.usersServiceBase
{
    private readonly ILogger<usersMessage> _logger;

    public usersMessage(ILogger<usersMessage> logger)
    {
        _logger = logger;
    }

    public override Task<MessageResponse> DoSomething(MyMessageRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Processing 'DoSomething' request: {Message}", request.Message);

        if (string.IsNullOrEmpty(request.Message))
        {
            throw new BadRequestException("Message cannot be empty.");
        }

        var responseBody = new MyMessageResponse
        {
            ResponseMessage = "You sent: " + request.Message
        };

        return Task.FromResult(MessageResponse.Ok(responseBody));
    }
}
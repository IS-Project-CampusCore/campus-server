using Grpc.Core;
using excelServiceClient;
using commons;
using commons.Protos;

namespace excel.Services;

public class excelMessage : excelService.excelServiceBase
{
    private readonly ILogger<excelMessage> _logger;

    public excelMessage(ILogger<excelMessage> logger)
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

        // Use the helper from your 'commons' project to pack the response
        return Task.FromResult(MessageResponse.Ok(responseBody));
    }
}
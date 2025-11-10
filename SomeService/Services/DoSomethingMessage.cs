using commons;
using commons.Protos;
using Grpc.Core;
using someServiceClient;

namespace SomeService.Services;

public class DoSomethingMessage(ServiceImplementation implementation,ILogger<DoSomethingMessage> logger) : someService.someServiceBase
{
    private readonly ILogger<DoSomethingMessage> _logger = logger;
    private readonly ServiceImplementation _serviceImplementation = implementation;

    public override Task<MessageResponse> DoSomething(DoSomethingReq request, ServerCallContext context)
    {
        _logger.LogInformation($"{nameof(DoSomethingMessage)} has begun");

        MessageResponse response = new MessageResponse();

        _logger.LogInformation($"{nameof(DoSomethingMessage)} Request: {nameof(DoSomethingReq)}={request.ToString()}");
        try
        {
            response = MessageResponse.Ok(_serviceImplementation.ProcessMessage(request.SomeReqMessage));

            _logger.LogInformation($"The request has been processed succesffuly: Response: {nameof(DoSomethingRes)}={response.ToString()}");
        }
        catch (ServiceMessageException ex)
        {
            _logger.LogError($"The request has failled with error: {ex.Message}");
            return Task.FromResult(ex.ToResponse());
        }

        return Task.FromResult(response);
    }
}

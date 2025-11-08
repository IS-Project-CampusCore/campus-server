using commons;
using Grpc.Core;
using someServiceClient;

namespace SomeService.Services;

public class DoSomethingMessage(ServiceImplementation implementation,ILogger<DoSomethingMessage> logger) : someService.someServiceBase
{
    private readonly ILogger<DoSomethingMessage> _logger = logger;
    private readonly ServiceImplementation _serviceImplementation = implementation;

    public override Task<DoSomethingRes> DoSomething(DoSomethingReq request, ServerCallContext context)
    {
        _logger.LogInformation($"{nameof(DoSomethingMessage)} has begun");

        DoSomethingRes response = new DoSomethingRes();

        _logger.LogInformation($"{nameof(DoSomethingMessage)} Request: {nameof(DoSomethingReq)}={request.ToString()}");
        try
        {
            response.SomeResMessage = _serviceImplementation.ProcessMessage(request.SomeReqMessage);
            _logger.LogInformation($"The request has been processed succesffuly: Response: {nameof(DoSomethingRes)}={response.ToString()}");
        }
        catch (ServiceMessageException ex)
        {
            response = new DoSomethingRes { SomeResMessage = ex.Message };
            _logger.LogError($"The request has failled with error: {ex.Message}");
            return Task.FromResult(response);
        }

        return Task.FromResult(response);
    }
}

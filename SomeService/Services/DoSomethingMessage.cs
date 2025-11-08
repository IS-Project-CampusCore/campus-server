using commons;
using commons.Protos;
using Grpc.Core;
using someServiceClient;
using Google.Protobuf.WellKnownTypes;

namespace SomeService.Services;

public class DoSomethingMessage(ServiceImplementation implementation,ILogger<DoSomethingMessage> logger) : someService.someServiceBase
{
    private readonly ILogger<DoSomethingMessage> _logger = logger;
    private readonly ServiceImplementation _serviceImplementation = implementation;

    public override Task<ProtoMessageResponse> DoSomething(DoSomethingReq request, ServerCallContext context)
    {
        _logger.LogInformation($"{nameof(DoSomethingMessage)} has begun");

        ProtoMessageResponse response = new ProtoMessageResponse();

        _logger.LogInformation($"{nameof(DoSomethingMessage)} Request: {nameof(DoSomethingReq)}={request.ToString()}");
        try
        {
            response.Body = Any.Pack(new DoSomethingRes { SomeResMessage = _serviceImplementation.ProcessMessage(request.SomeReqMessage) });
            _logger.LogInformation($"The request has been processed succesffuly: Response: {nameof(DoSomethingRes)}={response.ToString()}");
        }
        catch (ServiceMessageException ex)
        {
            response = new ProtoMessageResponse
            {
                Success = false,
            };
            _logger.LogError($"The request has failled with error: {ex.Message}");
            return Task.FromResult(response);
        }

        return Task.FromResult(response);
    }
}

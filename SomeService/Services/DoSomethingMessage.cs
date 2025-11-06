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
        DoSomethingRes doSomethingRes = new DoSomethingRes();

        try
        {
            doSomethingRes.SomeResMessage = _serviceImplementation.ProcessMessage(request.SomeReqMessage);
        }
        catch (ServiceMessageException ex)
        {
            doSomethingRes = new DoSomethingRes { SomeResMessage = ex.Message };
            return Task.FromResult(doSomethingRes);
        }

        return Task.FromResult(doSomethingRes);
    }
}

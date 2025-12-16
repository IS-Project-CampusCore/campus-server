using commons.Protos;
using __CAMEL_NAME__ServiceClient;
using Grpc.Core;
using MediatR;

namespace MyGrpcService;

public class MyGrpcServiceService(IMediator mediator) : __CAMEL_NAME__Service.__CAMEL_NAME__ServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> ExampleMessage(ExampleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}

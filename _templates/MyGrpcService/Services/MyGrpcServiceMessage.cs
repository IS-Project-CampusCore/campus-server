using Grpc.Core;
using commons;
using commons.Protos;
using __CAMEL_NAME__ServiceClient;
using MediatR;
using MyGrpcService.Implementation;

namespace MyGrpcService.Services;

public class ExampleMessage(
    ILogger<ExampleMessage> logger,
    MyGrpcServiceServiceImplementation implementation
) : IRequestHandler<ExampleRequest, MessageResponse>
{
    private readonly MyGrpcServiceServiceImplementation _impl = implementation;
    private readonly ILogger<ExampleMessage> _logger = logger;

    public Task<MessageResponse> Handle(ExampleRequest request, CancellationToken token)
    {
        return Task.FromResult(MessageResponse.Ok());
    }
}
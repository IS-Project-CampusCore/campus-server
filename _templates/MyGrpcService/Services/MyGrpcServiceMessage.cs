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
) : CampusMessage<ExampleRequest>(logger)
{
    private readonly MyGrpcServiceServiceImplementation _impl = implementation;

    protected override Task HandleMessage(ExampleRequest request, CancellationToken token) =>
        Task.CompletedTask;
}
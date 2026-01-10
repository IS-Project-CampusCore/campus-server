using Grpc.Core;
using commons;
using commons.Protos;
using announcementsServiceClient;
using MediatR;
using Announcements.Implementation;
using commons.RequestBase;

namespace Announcements.Services;

public class ExampleMessage(
    ILogger<ExampleMessage> logger,
    AnnouncementsServiceImplementation implementation
) : CampusMessage<ExampleRequest>(logger)
{
    private readonly AnnouncementsServiceImplementation _impl = implementation;

    protected override Task HandleMessage(ExampleRequest request, CancellationToken token) =>
        Task.CompletedTask;
}

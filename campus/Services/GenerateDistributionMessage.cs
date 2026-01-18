using campusServiceClient;
using campus.Implementation;
using commons.RequestBase;

namespace campus.Services;

public class GenerateDistributionMessage(
    ILogger<GenerateDistributionMessage> logger,
    CampusServiceImplementation implementation
) : CampusMessage<GenerateDistributionRequest>(logger)
{
    private readonly CampusServiceImplementation _impl = implementation;

    protected override Task HandleMessage(GenerateDistributionRequest request, CancellationToken token)
        => Task.CompletedTask;

}

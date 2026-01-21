using campus.Implementation;
using campus.Models;
using campusServiceClient;
using commons.RequestBase;

namespace campus.Services;

public class GetAccommodationsMessage(
ILogger<GetAccommodationsMessage> logger,
CampusServiceImplementation implementation
) : CampusMessage<GetAccsRequest, List<Accommodation>>(logger)
{
    private readonly CampusServiceImplementation _impl = implementation;

    protected override async Task<List<Accommodation>> HandleMessage(GetAccsRequest request, CancellationToken token)
        => await _impl.GetAccommodations();
}

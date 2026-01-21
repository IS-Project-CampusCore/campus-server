using campus.Implementation;
using campus.Models;
using campusServiceClient;
using commons.RequestBase;

namespace campus.Services;

public class GetAccommodationByIdMessage(
ILogger<GetAccommodationByIdMessage> logger,
CampusServiceImplementation implementation
) : CampusMessage<GetAccByIdRequest, Accommodation?>(logger)
{
    private readonly CampusServiceImplementation _impl = implementation;

    protected override async Task<Accommodation?> HandleMessage(GetAccByIdRequest request, CancellationToken token)
        => await _impl.GetAccommodationById(request.Id);
}

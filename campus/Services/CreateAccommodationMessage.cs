using campusServiceClient;
using campus.Implementation;
using campus.Models;
using commons.RequestBase;

namespace campus.Services;

public class CreateAccommodationMessage(
    ILogger<CreateAccommodationMessage> logger,
    CampusServiceImplementation implementation
) : CampusMessage<CreateAccommodationRequest, Accommodation>(logger)
{
    private readonly CampusServiceImplementation _impl = implementation;

    protected override async Task<Accommodation> HandleMessage(CreateAccommodationRequest request, CancellationToken token) 
        => await _impl.CreateAccommodationAsync(request.Name,request.Description,request.OpenTime,request.CloseTime);

}

using campusServiceClient;
using campus.Models;
using campus.Implementation;
using commons.RequestBase;

namespace campus.Services;

public class CreatePaymentMessage(
    ILogger<CreatePaymentMessage> logger,
    CampusServiceImplementation implementation
) : CampusMessage<CreatePaymentRequest,Payment>(logger)
{
    private readonly CampusServiceImplementation _impl = implementation;

    protected override async Task<Payment> HandleMessage(CreatePaymentRequest request, CancellationToken token) 
        => await _impl.CreatePaymentAsync(request.UserId,request.Amount);
}

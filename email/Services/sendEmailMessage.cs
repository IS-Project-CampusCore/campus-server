using commons.Protos;
using commons.RequestBase;
using email.Implementation;
using emailServiceClient;
using Grpc.Core;
using MediatR;

namespace email.Services;

public class SendEmailMessage(
    ILogger<SendEmailMessage> logger,
    EmailServiceImplementation implementation
) : CampusMessage<SendEmailRequest>(logger)
{
    private readonly EmailServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(SendEmailRequest request, CancellationToken token) => 
        await _impl.SendEmail(request);
}
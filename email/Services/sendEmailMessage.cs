using commons;
using commons.Protos;
using email.Implementation;
using emailServiceClient;
using Grpc.Core;
using MediatR;

namespace email.Services;

public class SendEmailMessage(
    ILogger<SendEmailMessage> logger,
    EmailServiceImplementation implementation
) : IRequestHandler<SendEmailRequest, MessageResponse>
{
    private readonly EmailServiceImplementation _impl = implementation;
    private readonly ILogger<SendEmailMessage> _logger = logger;

    public async Task<MessageResponse> Handle(SendEmailRequest request, CancellationToken token)
    {
        if (request is null || string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.ToName) || string.IsNullOrEmpty(request.TemplateName) || string.IsNullOrEmpty(request.TemplateData))
        {
            _logger.LogError("Send Email Request is empty");
            return MessageResponse.BadRequest("Send Email Request is empty");
        }

        try
        {
            await _impl.SendEmail(request); 

            return MessageResponse.Ok();
        }
        catch (ServiceMessageException ex)
        {
            return ex.ToResponse();
        }
    }
}
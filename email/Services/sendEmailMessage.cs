using commons;
using commons.Protos;
using emailServiceClient;
using Grpc.Core;

namespace email.Services;

public class SendEmailMessage(
    ILogger<SendEmailMessage> logger,
    EmailServiceImplementation implementation
) : emailService.emailServiceBase
{
    public override async Task<MessageResponse> SendEmail(SendEmailRequest request, ServerCallContext context)
    {
        logger.LogInformation($"Email Request: {request.ToString()}");

        try
        {
            await implementation.SendEmail(request); 

            logger.LogInformation($"The request has been processed succesffuly");
            return MessageResponse.Ok();
        }
        catch (ServiceMessageException ex)
        {
            logger.LogError($"The request has failled with error: {ex.Message}");
            return ex.ToResponse();
        }
    }
}
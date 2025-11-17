using commons;
using commons.Protos;
using emailServiceClient;
using Grpc.Core;

namespace email.Services;

public class sendEmailMessage(
    ILogger<sendEmailMessage> logger,
    EmailServiceImplementation implementation)
    : emailService.emailServiceBase
{

    public override async Task<MessageResponse> SendEmail(SendEmailRequest request, ServerCallContext context)
    {
        logger.LogInformation($"{nameof(sendEmailMessage)} has begun");
        logger.LogInformation($"{nameof(sendEmailMessage)} Request: {request.ToString()}");

        try
        {
            await implementation.ProcessEmailSend(request); 
            var response = MessageResponse.Ok("Email succesfully sent!");

            logger.LogInformation($"The request has been processed succesffuly: Response: {response.ToString()}");
            return response;
        }
        catch (ServiceMessageException ex)
        {
            logger.LogError($"The request has failled with error: {ex.Message}");
            return ex.ToResponse();
        }
    }
}
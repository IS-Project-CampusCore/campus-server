using commons;
using commons.Protos;
using emailServiceClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace email.Services
{
    public class EmailServiceImplementation(
        ILogger<EmailServiceImplementation> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {

        public async Task ProcessEmailSend(SendEmailRequest request)
        {
            logger.LogInformation("Processing SendEmail logic for {Email}", request.ToEmail);

            try
            {
                ValidateRequest(request);
                var httpContent = BuildBrevoPayload(request);
                await SendEmailToBrevoAsync(httpContent, request.ToEmail);
               
            }
            catch (ServiceMessageException ex)
            {
                logger.LogError(ex, "Failed to send email due to an unexpected error");
                throw new InternalErrorException("Failed to send email: " + ex.Message);
            }
        }

        private void ValidateRequest(SendEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.ToEmail) ||
                string.IsNullOrEmpty(request.Subject) ||
                string.IsNullOrEmpty(request.HtmlContent))
            {
                logger.LogWarning("SendEmail request failed: One of (ToEmail, Subject, HtmlContent) was empty.");
                throw new BadRequestException("ToEmail, Subject, and HtmlContent can not be empty.");
            }
        }

        private StringContent BuildBrevoPayload(SendEmailRequest request)
        {
            var payload = new
            {
                sender = new
                {
                    email = configuration["BrevoSettings:EmailFrom"],
                    name = configuration["BrevoSettings:SenderName"]
                },
                to = new[] { new { email = request.ToEmail, name = request.ToName } },
                subject = request.Subject,
                htmlContent = request.HtmlContent
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        }

        private async Task SendEmailToBrevoAsync(StringContent content, string toEmail)
        {
            var apiKey = configuration["BrevoSettings:ApiKey"];
            var apiUrl = configuration["BrevoSettings:ApiUrl"];
            var httpClient = httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("accept", "application/json");

            var brevoResponse = await httpClient.PostAsync(apiUrl, content);

            if (brevoResponse.IsSuccessStatusCode)
            {
                logger.LogInformation("Email succesfully sent to {Email}", toEmail);
            }
            else
            {
                var errorBody = await brevoResponse.Content.ReadAsStringAsync();
                logger.LogError("Brevo error: {StatusCode} - {ErrorBody}", brevoResponse.StatusCode, errorBody);
                throw new InternalErrorException($"Brevo error: {errorBody}");
            }
        }
    }
}
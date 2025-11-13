using commons;
using commons.Protos;
using emailServiceClient;
using Grpc.Core;
using System.Text;
using System.Text.Json;

namespace email.Services;

public class emailMessage : emailService.emailServiceBase
{
    private readonly ILogger<emailMessage> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public emailMessage(ILogger<emailMessage> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public override async Task<MessageResponse> SendEmail(SendEmailRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Procesing SendEmail request for {Email}", request.ToEmail);

        if (string.IsNullOrEmpty(request.ToEmail) ||
            string.IsNullOrEmpty(request.Subject) ||
            string.IsNullOrEmpty(request.HtmlContent))
        {
            _logger.LogWarning("SendEmail request failed: Unul din câmpuri (ToEmail, Subject, HtmlContent) e gol.");
            return MessageResponse.BadRequest("ToEmail, Subject, și HtmlContent nu pot fi goale.");
        }

        try
        {
            var apiKey = _configuration["BrevoSettings:ApiKey"];
            var apiUrl = _configuration["BrevoSettings:ApiUrl"];

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Clear(); 
            httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("accept", "application/json");

            var payload = new
            {
                sender = new { email = "rok1395b5@gmail.com", name = "CampusCore" },
                to = new[]
                    {
                        new { email = request.ToEmail, name = request.ToName }
                    },
                    subject = request.Subject,
                    htmlContent = request.HtmlContent
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var brevoResponse = await httpClient.PostAsync(apiUrl, content);

            if (brevoResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email trimis cu succes către {Email}", request.ToEmail);
                return MessageResponse.Ok("Email trimis cu succes.");
            }
            else
            {
                var errorBody = await brevoResponse.Content.ReadAsStringAsync();
                _logger.LogError("Eroare de la Brevo: {StatusCode} - {ErrorBody}", brevoResponse.StatusCode, errorBody);
                return MessageResponse.Error($"Eroare Brevo: {errorBody}", (int)brevoResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A eșuat trimiterea email-ului din cauza unei excepții neașteptate.");
            return MessageResponse.Error(ex);
        }
    }
}
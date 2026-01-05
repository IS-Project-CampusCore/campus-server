using commons.RequestBase;
using emailServiceClient;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace email.Implementation;

public class EmailServiceImplementation(
    ILogger<EmailServiceImplementation> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
{
    public async Task SendEmail(SendEmailRequest request)
    {
        logger.LogInformation("Processing SendEmail logic for {Email}", request.ToEmail);

        if (string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.TemplateName) || string.IsNullOrEmpty(request.TemplateData))
        {
            logger.LogWarning("SendEmail request failed: ToEmail, TemplateName, and TemplateData are required.");
            throw new BadRequestException("ToEmail, TemplateName, and TemplateData cannot be empty.");
        }

        try
        {
            var httpContent = BuildBrevoPayload(request);
            await SendEmailToBrevoAsync(httpContent, request.ToEmail);
        }
        catch (ServiceMessageException ex)
        {
            logger.LogError(ex, "Failed to send email due to an unexpected error");
            throw new InternalErrorException("Failed to send email: " + ex.Message);
        }
    }

    private StringContent BuildBrevoPayload(SendEmailRequest request)
    {
        string rawHtml = GetTemplateContent(request.TemplateName);

        string emailSubject = ExtractSubjectFromTemplate(rawHtml);
        string htmlWithData = PopulateTemplate(rawHtml, request.TemplateData);

        var payload = new
        {
            sender = new
            {
                email = configuration["BrevoSettings:EmailFrom"],
                name = configuration["BrevoSettings:SenderName"]
            },
            to = new[] { 
                new { 
                    email = request.ToEmail, 
                    name = request.ToName 
                } 
            },
            subject = emailSubject,
            htmlContent = htmlWithData
        };

        var jsonPayload = JsonSerializer.Serialize(payload);

        return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    }

    private string GetTemplateContent(string templateName)
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Templates", $"{templateName}.html");
            if (!File.Exists(path))
            {
                logger.LogError("Template file not found: {TemplatePath}", path);
                throw new InternalErrorException($"Template file '{templateName}.html' not found.");
            }

            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read template file {TemplateName}", templateName);
            throw new InternalErrorException($"Error reading template: {ex.Message}");
        }
    }

    private string ExtractSubjectFromTemplate(string htmlContent)
    {
        var match = Regex.Match(htmlContent, @"<!--\s*SUBJECT:\s*(.*?)\s*-->", RegexOptions.IgnoreCase);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        logger.LogWarning("Template is missing a subject comment. Using default subject.");
        throw new InternalErrorException("Template file is invalid: Subject comment is missing.");
    }

    private string PopulateTemplate(string htmlContent, string jsonData)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

            if (data == null)
            {
                return htmlContent;
            }

            foreach (var entry in data)
            {
                htmlContent = htmlContent.Replace($"{{{{{entry.Key.ToLower()}}}}}", entry.Value);
            }

            return htmlContent;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize TemplateData JSON");
            throw new BadRequestException("TemplateData is not valid JSON.");
        }
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
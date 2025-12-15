using commons.Protos;
using emailServiceClient;
using FastEndpoints;
using System.Text.Json;

namespace http.Endpoints.Email;

public record TemplateData(string Name);

public record SendEmailApiRequest(
    string ToEmail,
    string ToName,
    string TemplateName,
    JsonElement TemplateData
);


public class SendEmail(ILogger<SendEmail> logger) : CampusEndpoint<SendEmailApiRequest>(logger)
{
    public emailService.emailServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/email/send"); 
        AllowAnonymous();
    }

    public override async Task HandleAsync(SendEmailApiRequest req, CancellationToken cancellationToken)
    {
        string templateDataString = JsonSerializer.Serialize(req.TemplateData);

        SendEmailRequest grpcRequest = new SendEmailRequest
        {
            ToEmail = req.ToEmail,
            ToName = req.ToName ?? "",
            TemplateName = req.TemplateName,
            TemplateData = templateDataString
        };

        MessageResponse grpcResponse = await Client.SendEmailAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
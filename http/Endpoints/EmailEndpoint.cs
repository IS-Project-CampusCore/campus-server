

using commons.Protos; 
using emailServiceClient; 
using FastEndpoints;
using System.Text.Json;

namespace http.Endpoints;

public record TemplateData(string Name);

public record SendEmailApiRequest(
    string ToEmail,
    string ToName,
    string TemplateName,
    JsonElement TemplateData
);


public class EmailEndpoint : Endpoint<SendEmailApiRequest, string>
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

        var grpcRequest = new SendEmailRequest
        {
            ToEmail = req.ToEmail,
            ToName = req.ToName ?? "",
            TemplateName = req.TemplateName,
            TemplateData = templateDataString
        };

        try
        {
            var apiRes = await Client.SendEmailAsync(grpcRequest, null, null, cancellationToken);

            if (apiRes.Success)
            {
                await Send.OkAsync(apiRes.Body);
                return;
            }

            await Send.ErrorsAsync(apiRes.Code);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500);
        }
    }
}
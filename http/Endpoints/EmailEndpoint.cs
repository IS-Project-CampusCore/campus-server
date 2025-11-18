

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

    public override async Task HandleAsync(SendEmailApiRequest req, CancellationToken ct)
    {
        //MessageBody requestData = new MessageBody(req.TemplateData);
        //string? templateDataString = requestData.TryGetString("Name") ;
        string templateDataString = JsonSerializer.Serialize(req.TemplateData);

        var grpcRequest = new SendEmailRequest
        {
            ToEmail = req.ToEmail,
            ToName = req.ToName ?? "",
            TemplateName = req.TemplateName,
            TemplateData = templateDataString
        };

        MessageResponse apiRes;
        try
        {
            apiRes = await Client.SendEmailAsync(grpcRequest, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await Send.ErrorsAsync(500); 
            return;
        }

        if (apiRes.Success)
        {
            await Send.OkAsync(apiRes.Body); 
        }
        else
        {
            await Send.ErrorsAsync(apiRes.Code); 
        }
    }
}
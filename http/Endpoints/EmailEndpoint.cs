

using commons.Protos; 
using FastEndpoints;
using emailServiceClient; 

namespace http.Endpoints;

public record SendEmailApiRequest(
    string ToEmail,
    string ToName,
    string Subject,
    string Body
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
        var grpcRequest = new SendEmailRequest
        {
            ToEmail = req.ToEmail,
            ToName = req.ToName ?? "", 
            Subject = req.Subject,
            HtmlContent = req.Body 
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
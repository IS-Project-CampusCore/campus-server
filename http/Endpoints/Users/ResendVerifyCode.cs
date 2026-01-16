using commons.Protos;
using usersServiceClient;

namespace http.Endpoints.Users; 

public record EmailRequest(string Email);

public class ResendVerifyCode(ILogger<ResendVerifyCode> logger) : CampusEndpoint<EmailRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/resend-code"); 
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmailRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
        {
            await HandleErrorsAsync(400, "Email is required", cancellationToken);
            return;
        }

        var grpcRequest = new ResendVerifyCodeRequest
        {
            Email = req.Email
        };

        MessageResponse response = await Client.ResendVerifyCodeAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
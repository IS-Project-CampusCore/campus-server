using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users; 

public record ResendCodeRequest(string Email);

public class ResendVerifyCode(ILogger<ResendVerifyCode> logger) : CampusEndpoint<ResendCodeRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/resend-code"); 
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResendCodeRequest req, CancellationToken cancellationToken)
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
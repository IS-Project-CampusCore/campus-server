using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class ResetPassword(ILogger<ResetPassword> logger) : CampusEndpoint<EmailRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/reset-password");
        AllowAnonymous(); 
    }

    public override async Task HandleAsync(EmailRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
        {
            await HandleErrorsAsync(400, "Email is required", cancellationToken);
            return;
        }

        var grpcRequest = new ResetPasswordRequest
        {
            Email = req.Email
        };

        MessageResponse response = await Client.ResetPasswordAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
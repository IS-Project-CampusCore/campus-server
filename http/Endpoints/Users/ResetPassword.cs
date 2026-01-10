using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class ResetPassword(ILogger<ResetPassword> logger) : CampusEndpoint<ResetPasswordRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/reset-password");
        AllowAnonymous(); 
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken cancellationToken)
    {
        MessageResponse response = await Client.ResetPasswordAsync(req, null, null, cancellationToken);

        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
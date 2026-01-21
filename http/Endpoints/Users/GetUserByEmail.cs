using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUserByEmail(ILogger<GetUserByEmail> logger) : CampusEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/email");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "User email not found", cancellationToken);
            return;
        }

        var grpcRequest = new UserEmailRequest
        {
            Email = req
        };

        MessageResponse response = await Client.GetUserByEmailAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
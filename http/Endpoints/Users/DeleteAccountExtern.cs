using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;


public class DeleteAccountExtern(ILogger<DeleteAccount> logger) : CampusEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Delete("api/users/delete-ext");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("management");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(401, "User ID not found in token", cancellationToken);
            return;
        }

        var grpcRequest = new DeleteAccountRequest
        {
            UserId = req
        };

        MessageResponse response = await Client.DeleteAccountAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
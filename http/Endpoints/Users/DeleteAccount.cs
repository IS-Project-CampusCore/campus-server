using commons.Protos;
using FastEndpoints; 
using http.Auth;
using usersServiceClient;
using System.Security.Claims;

namespace http.Endpoints.Users;


public class DeleteAccount(ILogger<DeleteAccount> logger) : CampusEndpoint<EmptyRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Delete("api/users/delete");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            await HandleErrorsAsync(401, "User ID not found in token", cancellationToken);
            return;
        }

        var grpcRequest = new DeleteAccountRequest
        {
            UserId = userId
        };

        MessageResponse response = await Client.DeleteAccountAsync(grpcRequest, null, null, cancellationToken);

        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
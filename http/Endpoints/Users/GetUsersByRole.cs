using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUsersByRole(ILogger<GetUsersByRole> logger) : CampusEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/role");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "User Role not found", cancellationToken);
            return;
        }

        var grpcRequest = new UsersRoleRequest
        {
            Role = req
        };

        MessageResponse response = await Client.GetUsersByRoleAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
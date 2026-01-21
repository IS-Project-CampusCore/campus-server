using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUserById(ILogger<GetUserById> logger) : CampusEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/user");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "User ID not found", cancellationToken);
            return;
        }

        var grpcRequest = new UserIdRequest
        {
            Id = req
        };

        MessageResponse response = await Client.GetUserByIdAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
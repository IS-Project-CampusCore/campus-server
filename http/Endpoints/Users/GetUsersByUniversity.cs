using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUsersByUniversity(ILogger<GetUsersByUniversity> logger) : CampusEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/university");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "User University not found", cancellationToken);
            return;
        }

        var grpcRequest = new UsersUniversityRequest
        {
            University = req
        };

        MessageResponse response = await Client.GetUsersByUniversityAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
using commons.Protos;
using FastEndpoints;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetAllUsers(ILogger<GetAllUsers> logger) : CampusEndpoint<EmptyRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/all");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var grpcRequest = new GetAllUsersRequest
        {
            Placeholder = "ignored"
        };

        MessageResponse response = await Client.GetAllUsersAsync(grpcRequest, null, null, cancellationToken);

        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
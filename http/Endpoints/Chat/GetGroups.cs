using http.Auth;
using chatServiceClient;
using commons.Protos;
using FastEndpoints;

namespace http.Endpoints.Chat;

public class GetGroups(ILogger<GetGroups> logger) : CampusEndpoint(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/groups");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var grpcRequest = new GetGroupsRequest
        {
            MemberId = GetUserId()
        };

        MessageResponse grpcResponse = await Client.GetUserGroupsAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

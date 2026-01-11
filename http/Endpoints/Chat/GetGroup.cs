using FastEndpoints;
using http.Auth;
using commons.Protos;
using chatServiceClient;

namespace http.Endpoints.Chat;

public class GetGroup(ILogger<GetGroup> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/group");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("student", "professor", "campus_student");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetGroupRequest
        {
            GroupId = req
        };

        MessageResponse grpcResponse = await Client.GetGroupAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

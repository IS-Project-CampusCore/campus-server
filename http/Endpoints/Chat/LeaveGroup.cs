using http.Auth;
using chatServiceClient;
using commons.Protos;

namespace http.Endpoints.Chat;

public class LeaveGroup(ILogger<LeaveGroup> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/chat/leave");
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
        var memberId = GetUserId();

        var grpcRequest = new LeaveGroupRequest
        {
            GroupId = req,
            MemberId = memberId,
        };

        MessageResponse grpcResponse = await Client.LeaveGroupAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

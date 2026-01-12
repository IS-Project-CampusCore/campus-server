using chatServiceClient;
using commons.Protos;
using http.Auth;

namespace http.Endpoints.Chat;

public class RemoveMember(ILogger<RemoveMember> logger) : CampusEndpoint<MemberRequest>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/chat/remove");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("student", "professor", "campus_student");
    }

    public override async Task HandleAsync(MemberRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.GroupId) || string.IsNullOrEmpty(req.MemberId))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new RemoveMemberRequest
        {
            GroupId = req.GroupId,
            MemberId = req.MemberId,
            RemovedById = GetUserId()
        };

        MessageResponse grpcResponse = await Client.RemoveGroupMemberAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

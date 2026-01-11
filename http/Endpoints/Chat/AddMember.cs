using chatServiceClient;
using commons.Protos;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Chat;

public record MemberRequest(string GroupId, string MemberId);

public class AddMember(ILogger<AddMember> logger) : CampusEndpoint<MemberRequest>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/chat/add");
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

        var grpcRequest = new AddMemberRequest
        {
            GroupId = req.GroupId,
            MemberId = req.MemberId,
        };

        MessageResponse grpcResponse = await Client.AddGroupMemberAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

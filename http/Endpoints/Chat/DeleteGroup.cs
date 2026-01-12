using http.Auth;
using chatServiceClient;
using commons.Protos;

namespace http.Endpoints.Chat;

public class DeleteGroup(ILogger<DeleteGroup> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/chat/group-delete");
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

        string adminId = GetUserId();

        var grpcRequest = new DeleteGroupRequest
        {
            GroupId = req,
            AdminId = adminId,
        };

        MessageResponse grpcResponse = await Client.DeleteGroupAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

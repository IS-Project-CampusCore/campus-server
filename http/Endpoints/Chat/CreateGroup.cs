using chatServiceClient;
using commons.Protos;
using http.Auth;

namespace http.Endpoints.Chat;


public record GroupNameRequest(string Name);

public class CreateGroup(ILogger<CreateGroup> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/chat/group-create");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("student", "professor", "campus_student");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        string adminId = GetUserId();

        if (req is null || string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new CreateGroupRequest
        {
            Name = req,
            AdminId = adminId,
        };

        MessageResponse grpcResponse = await Client.CreateGroupAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

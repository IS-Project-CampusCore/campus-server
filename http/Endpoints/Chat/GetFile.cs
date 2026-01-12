using http.Auth;
using chatServiceClient;
using commons.Protos;

namespace http.Endpoints.Chat;

public class GetFile(ILogger<GetFile> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/file");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("student", "professor", "campus_student");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetFileRequest
        {
            FileId = req
        };

        MessageResponse grpcResponse = await Client.GetFileAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

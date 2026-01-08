using http.Auth;
using chatServiceClient;
using commons.Protos;

namespace http.Endpoints.Chat;

public class GetFiles(ILogger<GetFiles> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/files");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetMessageFilesRequest
        {
            MessageId = req
        };

        MessageResponse grpcResponse = await Client.GetMessageFilesAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

using http.Auth;
using commons.Protos;
using chatServiceClient;

namespace http.Endpoints.Chat;

public record GetHistoryApiRequest(string GroupId, int Skip, int Limit);
public class GetHistory(ILogger<SendMessage> logger) : CampusEndpoint<GetHistoryApiRequest>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/history");
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(GetHistoryApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.GroupId))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        if (req.Skip < 0 || req.Limit < 1 || req.Limit > 20)
        {
            await HandleErrorsAsync(400, "Invalid Skip and Limit", cancellationToken);
            return;
        }

        var grpcRequest = new GetHistoryRequest
        {
            ReciverId = GetUserId(),
            GroupId = req.GroupId,
            Skip = req.Skip,
            Limit = req.Limit
        };

        MessageResponse grpcResponse = await Client.GetHistoryAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
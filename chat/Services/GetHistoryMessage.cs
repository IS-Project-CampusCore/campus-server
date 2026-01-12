using chat.Models;
using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class GetHistoryMessage(
    ILogger<GetUserGroupsMessage> logger,
    IChatService implementation
) : CampusMessage<GetHistoryRequest, IEnumerable<ChatMessage>>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<IEnumerable<ChatMessage>> HandleMessage(GetHistoryRequest request, CancellationToken cancellationToken)
        => await _impl.GetMessagesAsync(request.ReciverId, request.GroupId, request.Skip, request.Limit);
}
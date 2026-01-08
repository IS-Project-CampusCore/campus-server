using chat.Models;
using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class GetHistoryMessage(
    ILogger<GetUserGroupsMessage> logger,
    ChatServiceImplementation implementation
) : CampusMessage<GetHistoryRequest, IEnumerable<ChatMessage>>(logger)
{
    private readonly ChatServiceImplementation _impl = implementation;

    protected override async Task<IEnumerable<ChatMessage>> HandleMessage(GetHistoryRequest request, CancellationToken cancellationToken)
        => await _impl.GetMessagesAsync(request.ReciverId, request.GroupId, request.Skip, request.Limit);
}
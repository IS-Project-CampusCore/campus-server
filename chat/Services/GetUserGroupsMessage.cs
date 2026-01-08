using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;
using chat.Models;

namespace chat.Services;

public class GetUserGroupsMessage(
    ILogger<GetUserGroupsMessage> logger,
    ChatServiceImplementation implementation
) : CampusMessage<GetGroupsRequest, IEnumerable<Group>?>(logger)
{
    private readonly ChatServiceImplementation _impl = implementation;

    protected override async Task<IEnumerable<Group>?> HandleMessage(GetGroupsRequest request, CancellationToken cancellationToken)
        => await _impl.GetUserGroupsAsync(request.MemberId);
}
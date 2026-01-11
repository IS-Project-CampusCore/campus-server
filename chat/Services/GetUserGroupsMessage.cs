using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;
using chat.Models;

namespace chat.Services;

public class GetUserGroupsMessage(
    ILogger<GetUserGroupsMessage> logger,
    IChatService implementation
) : CampusMessage<GetUserGroupsRequest, IEnumerable<Group>?>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<IEnumerable<Group>?> HandleMessage(GetUserGroupsRequest request, CancellationToken cancellationToken)
        => await _impl.GetUserGroupsAsync(request.MemberId);
}
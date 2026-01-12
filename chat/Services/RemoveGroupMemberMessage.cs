using Chat.Implementation;
using commons.RequestBase;
using chatServiceClient;

namespace chat.Services;

public class RemoveGroupMemberMessage(
    ILogger<RemoveGroupMemberMessage> logger,
    IChatService implementation
) : CampusMessage<RemoveMemberRequest>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task HandleMessage(RemoveMemberRequest request, CancellationToken cancellationToken)
        => await _impl.RemoveGroupMemberAsync(request.GroupId, request.MemberId, request.RemovedById);
}

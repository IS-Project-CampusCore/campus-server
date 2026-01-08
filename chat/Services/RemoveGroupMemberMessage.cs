using Chat.Implementation;
using commons.RequestBase;
using chatServiceClient;

namespace chat.Services;

public class RemoveGroupMemberMessage(
    ILogger<RemoveGroupMemberMessage> logger,
    ChatServiceImplementation implementation
) : CampusMessage<RemoveMemberRequest>(logger)
{
    private readonly ChatServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(RemoveMemberRequest request, CancellationToken cancellationToken)
        => await _impl.RemoveGroupMemberAsync(request.GroupId, request.MemberId);
}

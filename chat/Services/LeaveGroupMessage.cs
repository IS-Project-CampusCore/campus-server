using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class LeaveGroupMessage(
    ILogger<LeaveGroupMessage> logger,
    IChatService implementation
) : CampusMessage<LeaveGroupRequest>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task HandleMessage(LeaveGroupRequest request, CancellationToken token) =>
        await _impl.LeaveGroupAsync(request.GroupId, request.MemberId);
}

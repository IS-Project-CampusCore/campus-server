using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class AddGroupMemberMessage(
    ILogger<AddGroupMemberMessage> logger,
    IChatService implementation
) : CampusMessage<AddMemberRequest>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task HandleMessage(AddMemberRequest request, CancellationToken cancellationToken)
        => await _impl.AddGroupMemberAsync(request.GroupId, request.MemberId);
}


using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class GetGroupMembersMessage(
    ILogger<GetGroupMembersMessage> logger,
    IChatService implementation
) : CampusMessage<GetGroupMembersRequest, IEnumerable<string>>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<IEnumerable<string>> HandleMessage(GetGroupMembersRequest request, CancellationToken cancellationToken)
        => await _impl.GetGroupMembersAsync(request.GroupId);
}

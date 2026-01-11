using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;
using chat.Models;

namespace chat.Services;

public class GetGroupMessage(
    ILogger<GetGroupMessage> logger,
    IChatService implementation
) : CampusMessage<GetGroupRequest, Group>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<Group> HandleMessage(GetGroupRequest request, CancellationToken cancellationToken)
        => await _impl.GetGroupByIdAsync(request.GroupId);
}

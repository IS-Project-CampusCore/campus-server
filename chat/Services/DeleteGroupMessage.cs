using Chat.Implementation;
using Chat.Services;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class DeleteGroupMessage(
    ILogger<DeleteGroupMessage> logger,
    IChatService implementation
) : CampusMessage<DeleteGroupRequest>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task HandleMessage(DeleteGroupRequest request, CancellationToken token) =>
        await _impl.DeleteGroupAsync(request.GroupId, request.AdminId);
}

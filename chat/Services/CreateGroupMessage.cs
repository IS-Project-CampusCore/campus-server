using chatServiceClient;
using Chat.Implementation;
using commons.RequestBase;
using chat.Models;

namespace Chat.Services;

public class CreateGroupMessage(
    ILogger<CreateGroupMessage> logger,
    ChatServiceImplementation implementation
) : CampusMessage<CreateGroupRequest, Group>(logger)
{
    private readonly ChatServiceImplementation _impl = implementation;

    protected override async Task<Group> HandleMessage(CreateGroupRequest request, CancellationToken token) =>
        await _impl.CreateGroupAsync(request.Name, request.AdminId);
}

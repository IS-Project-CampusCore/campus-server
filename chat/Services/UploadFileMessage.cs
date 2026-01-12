using chat.Models;
using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class UploadFileMessage(
    ILogger<UploadFileMessage> logger,
    IChatService implementation
) : CampusMessage<UploadFileRequest, ChatFile>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<ChatFile> HandleMessage(UploadFileRequest request, CancellationToken cancellationToken)
        => await _impl.UploadFileAsync(request.Name, request.GroupId, [.. request.Data]);
}

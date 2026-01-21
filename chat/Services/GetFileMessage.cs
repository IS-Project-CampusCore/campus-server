using chat.Models;
using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class GetFileMessage(
    ILogger<GetFileMessage> logger,
    IChatService implementation
) : CampusMessage<GetFileRequest, FileResponse>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<FileResponse> HandleMessage(GetFileRequest request, CancellationToken cancellationToken)
        => await _impl.GetFileByIdAsync(request.FileId);
}

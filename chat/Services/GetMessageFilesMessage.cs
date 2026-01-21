using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;
using chat.Models;

namespace chat.Services;

public class GetMessageFilesMessage(
    ILogger<GetMessageFilesMessage> logger,
    IChatService implementation
) : CampusMessage<GetMessageFilesRequest, IEnumerable<FileResponse>?>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<IEnumerable<FileResponse>?> HandleMessage(GetMessageFilesRequest request, CancellationToken cancellationToken)
        => await _impl.GetFilesByMessageIdAsync(request.MessageId);
}

using Chat.Implementation;
using chatServiceClient;
using commons.RequestBase;

namespace chat.Services;

public class GetMessageFilesMessage(
    ILogger<GetMessageFilesMessage> logger,
    IChatService implementation
) : CampusMessage<GetMessageFilesRequest, IEnumerable<byte[]>?>(logger)
{
    private readonly IChatService _impl = implementation;

    protected override async Task<IEnumerable<byte[]>?> HandleMessage(GetMessageFilesRequest request, CancellationToken cancellationToken)
        => await _impl.GetFilesByMessageIdAsync(request.MessageId);
}

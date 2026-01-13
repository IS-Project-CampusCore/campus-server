using announcements.Implementation;
using announcementsServiceClient;
using commons.RequestBase;

namespace announcements.Services;

public class DeleteAnnouncementMessage(
    ILogger<DeleteAnnouncementMessage> logger,
    AnnouncementServiceImplementation implementation
) : CampusMessage<DeleteAnnouncementRequest, bool>(logger)
{
    private readonly AnnouncementServiceImplementation _impl = implementation;

    protected override async Task<bool> HandleMessage(DeleteAnnouncementRequest request, CancellationToken token)
        => await _impl.DeleteAsync(request.Id);
}
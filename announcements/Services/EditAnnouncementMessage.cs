using announcements.Implementation;
using Announcements.Model;
using announcementsServiceClient;
using commons.RequestBase;

namespace announcements.Services;

public class EditAnnouncementMessage(
    ILogger<EditAnnouncementMessage> logger,
    AnnouncementServiceImplementation implementation
) : CampusMessage<EditAnnouncementRequest, Announcement>(logger)
{
    private readonly AnnouncementServiceImplementation _impl = implementation;

    protected override async Task<Announcement> HandleMessage(EditAnnouncementRequest request, CancellationToken token)
        => await _impl.EditAsync(request);
}
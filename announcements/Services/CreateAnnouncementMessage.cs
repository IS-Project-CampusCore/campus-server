using announcements.Implementation;
using Announcements.Model;
using announcementsServiceClient;
using commons.RequestBase;

namespace announcements.Services;

public class CreateAnnouncementMessage(
    ILogger<CreateAnnouncementMessage> logger,
    AnnouncementServiceImplementation implementation
) : CampusMessage<CreateAnnouncementRequest, Announcement>(logger)
{
    private readonly AnnouncementServiceImplementation _impl = implementation;

    protected override async Task<Announcement> HandleMessage(CreateAnnouncementRequest request, CancellationToken token)
        => await _impl.CreateAsync(request);
}
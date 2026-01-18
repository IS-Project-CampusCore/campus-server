using announcements.Implementation;
using Announcements.Model;
using announcementsServiceClient;
using commons.RequestBase;

namespace announcements.Services;

public class GetAnnouncementsMessage(
    ILogger<GetAnnouncementsMessage> logger,
    AnnouncementServiceImplementation implementation
) : CampusMessage<GetAnnRequest, List<Announcement>>(logger)
{
    private readonly AnnouncementServiceImplementation _impl = implementation;

    protected override async Task<List<Announcement>> HandleMessage(GetAnnRequest request, CancellationToken token)
        => await _impl.GetAnnouncementsAsync();
}
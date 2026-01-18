using announcementsServiceClient;
using commons.Protos;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Announcements;

public class GetAnnouncements(ILogger<EditAnnouncement> logger) : CampusEndpoint(logger)
{
    public announcementsService.announcementsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/announcements/announcements");
        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        MessageResponse response = await Client.GetAnnouncementsAsync(new GetAnnRequest { }, null, null, cancellationToken);
        await SendAsync(response, cancellationToken);
    }
}
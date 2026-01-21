using announcementsServiceClient;
using commons.Protos;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Announcements;

public class DeleteAnnouncement(ILogger<DeleteAnnouncement> logger) : CampusEndpoint<string>(logger)
{
    public announcementsService.announcementsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/announcements/delete"); 
        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Announcement ID is required", cancellationToken);
            return;
        }

        var grpcRequest = new DeleteAnnouncementRequest
        {
            Id = req
        };

        MessageResponse response = await Client.DeleteAnnouncementAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken);
    }
}
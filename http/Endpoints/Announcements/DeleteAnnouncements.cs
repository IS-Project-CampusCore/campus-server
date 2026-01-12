using announcementsServiceClient;
using commons.Protos;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Announcements;

public record DeleteAnnouncementApiRequest(string Id);

public class DeleteAnnouncement(ILogger<DeleteAnnouncement> logger) : CampusEndpoint<DeleteAnnouncementApiRequest>(logger)
{
    public announcementsService.announcementsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Delete("api/announcements/{Id}"); 
        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(DeleteAnnouncementApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Id))
        {
            await HandleErrorsAsync(400, "Announcement ID is required", cancellationToken);
            return;
        }

        var grpcRequest = new DeleteAnnouncementRequest
        {
            Id = req.Id
        };

        MessageResponse response = await Client.DeleteAnnouncementAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken);
    }
}
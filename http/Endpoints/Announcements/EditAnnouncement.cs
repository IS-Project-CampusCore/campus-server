using announcementsServiceClient;
using commons.Protos;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Announcements;

public record EditAnnouncementApiRequest(string Id, string Title, string Message);

public class EditAnnouncement(ILogger<EditAnnouncement> logger) : CampusEndpoint<EditAnnouncementApiRequest>(logger)
{
    public announcementsService.announcementsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/announcements/edit");
        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(EditAnnouncementApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Id) || string.IsNullOrEmpty(req.Title) || string.IsNullOrEmpty(req.Message))
        {
            await HandleErrorsAsync(400, "Id, Title and Message are required", cancellationToken);
            return;
        }

        var grpcRequest = new EditAnnouncementRequest
        {
            Id = req.Id,
            NewTitle = req.Title,
            NewMessage = req.Message
        };

        MessageResponse response = await Client.EditAnnouncementAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken);
    }
}
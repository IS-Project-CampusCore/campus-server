using announcementsServiceClient;
using commons.Protos;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Announcements;

public record CreateAnnouncementApiRequest(string Title, string Message);

public class CreateAnnouncement(ILogger<CreateAnnouncement> logger) : CampusEndpoint<CreateAnnouncementApiRequest>(logger)
{
    public announcementsService.announcementsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/announcements/create");
        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(CreateAnnouncementApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Title) || string.IsNullOrEmpty(req.Message))
        {
            await HandleErrorsAsync(400, "Title and Message are required", cancellationToken);
            return;
        }

        string userId = GetUserId();

        var grpcRequest = new CreateAnnouncementRequest
        {
            Title = req.Title,
            Message = req.Message,
            Author = userId
        };

        MessageResponse response = await Client.CreateAnnouncementAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(response, cancellationToken);
    }
}
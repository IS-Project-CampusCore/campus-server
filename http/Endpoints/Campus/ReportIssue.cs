using campusServiceClient;
using http.Auth;
using commons.Protos;


namespace http.Endpoints.Campus;

public record ReportIssueApiRequest(string Location, string Description);

public class ReportIssue(ILogger<ReportIssue> logger) : CampusEndpoint<ReportIssueApiRequest>(logger)
{
    public campusService.campusServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/campus/report-issue");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles( "campus_student");
    }

    public override async Task HandleAsync(ReportIssueApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || (string.IsNullOrEmpty(req.Location) && string.IsNullOrEmpty(req.Description)))
        {
            await HandleErrorsAsync(400, "Location or Description must be provided", cancellationToken);
            return;
        }

        var issuerId = GetUserId();

        var grpcRequest = new ReportIssueRequest
        {
            IssuerId = issuerId,
            Location = req.Location ?? string.Empty,
            Description = req.Description ?? string.Empty
        };

        MessageResponse grpcResponse = await Client.ReportIssueAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
using http.Auth;
using campusServiceClient;
using commons.Protos;
using FastEndpoints;

namespace http.Endpoints.Campus;

public class GetAccommodations(ILogger<ReportIssue> logger) : CampusEndpoint(logger)
{
    public campusService.campusServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/campus/accs");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("campus_student");
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        MessageResponse grpcResponse = await Client.GetAccommodationsAsync(new GetAccsRequest { Empty = "" }, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

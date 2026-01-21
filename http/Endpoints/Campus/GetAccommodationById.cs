using http.Auth;
using campusServiceClient;
using commons.Protos;

namespace http.Endpoints.Campus;

public class GetAccommodationById(ILogger<ReportIssue> logger) : CampusEndpoint<string>(logger)
{
    public campusService.campusServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/campus/acc");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("campus_student", "management");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetAccByIdRequest
        {
            Id = req
        };

        MessageResponse grpcResponse = await Client.GetAccommodationByIdAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

using http.Auth;
using scheduleServiceClient;
using commons.Protos;

namespace http.Endpoints.Schedule;

public class GetAccSchedule(ILogger<GetAccSchedule> logger) : CampusEndpoint<string>(logger)
{
    public scheduleService.scheduleServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/schedule/acc");

        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetAccScheduleRequest
        {
            Name = req
        };

        MessageResponse grpcResponse = await Client.GetAccommodationScheduleAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

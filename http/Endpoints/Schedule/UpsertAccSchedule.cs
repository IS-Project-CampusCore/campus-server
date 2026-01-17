using http.Auth;
using scheduleServiceClient;
using commons.Protos;

namespace http.Endpoints.Schedule;

public record AccScheduleRequest(string FileName, string Name);
public class UpsertAccSchedule(ILogger<UpsertAccSchedule> logger) : CampusEndpoint<AccScheduleRequest>(logger)
{
    public scheduleService.scheduleServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/schedule/add-acc");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(AccScheduleRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName) || string.IsNullOrEmpty(req.Name))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new UpsertAccScheduleRequest
        {
            FileName = req.FileName,
            Name = req.Name
        };

        MessageResponse grpcResponse = await Client.UpsertAccommodationScheduleAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

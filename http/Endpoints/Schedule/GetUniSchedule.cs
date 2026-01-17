using http.Auth;
using scheduleServiceClient;
using commons.Protos;

namespace http.Endpoints.Schedule;

public record GetUniScheduleApiRequest(string University, string Major, int Year, int Group);
public class GetUniSchedule(ILogger<GetUniSchedule> logger) : CampusEndpoint<GetUniScheduleApiRequest>(logger)
{
    public scheduleService.scheduleServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/schedule/uni");

        Policies(CampusPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(GetUniScheduleApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.University) || string.IsNullOrEmpty(req.Major))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetUniScheduleRequest
        {
            University = req.University,
            Major = req.Major,
            Year = req.Year,
            Group = req.Group
        };

        MessageResponse grpcResponse = await Client.GetUniversityScheduleAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

using http.Auth;
using scheduleServiceClient;
using commons.Protos;

namespace http.Endpoints.Schedule;

public record UniScheduleRequest(string FileName, string University, string Major, int Year, int Groups);

public class UpsertUniSchedule(ILogger<UpsertUniSchedule> logger) : CampusEndpoint<UniScheduleRequest>(logger)
{
    public scheduleService.scheduleServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/schedule/add-uni");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(UniScheduleRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName) || string.IsNullOrEmpty(req.University) || string.IsNullOrEmpty(req.Major))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new UpsertUniScheduleRequest
        {
            FileName = req.FileName,
            University = req.University,
            Major = req.Major,
            Year = req.Year,
            Groups = req.Groups
        };

        MessageResponse grpcResponse = await Client.UpsertUniversityScheduleAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

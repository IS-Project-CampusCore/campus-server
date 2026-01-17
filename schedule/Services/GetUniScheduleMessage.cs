using commons.RequestBase;
using schedule.Implementation;
using schedule.Models;
using scheduleServiceClient;

namespace schedule.Services;

public class GetUniScheduleMessage(
    ILogger<UpsertUniScheduleMessage> logger,
    ScheduleServiceImplementation implementation
) : CampusMessage<GetUniScheduleRequest, List<UniversitySchedule>>(logger)
{
    private readonly ScheduleServiceImplementation _impl = implementation;

    protected override async Task<List<UniversitySchedule>> HandleMessage(GetUniScheduleRequest request, CancellationToken token)
        => await _impl.GetUniversitySchedulesAsync(request.University, request.Major, request.Year, request.Group);
}

using commons.RequestBase;
using schedule.Implementation;
using schedule.Models;
using scheduleServiceClient;

namespace schedule.Services;

public class UpsertUniScheduleMessage(
    ILogger<UpsertUniScheduleMessage> logger,
    ScheduleServiceImplementation implementation
) : CampusMessage<UpsertUniScheduleRequest, BulkResult<List<UniversitySchedule>>>(logger)
{
    private readonly ScheduleServiceImplementation _impl = implementation;

    protected override async Task<BulkResult<List<UniversitySchedule>>> HandleMessage(UpsertUniScheduleRequest request, CancellationToken token)
        => await _impl.UpsertScheduleAsync(request.FileName, request.University, request.Major, request.Year, request.Groups);
}

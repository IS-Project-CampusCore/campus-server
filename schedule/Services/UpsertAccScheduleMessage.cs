using commons.RequestBase;
using schedule.Implementation;
using schedule.Models;
using scheduleServiceClient;

namespace schedule.Services;

public class UpsertAccScheduleMessage(
    ILogger<UpsertUniScheduleMessage> logger,
    ScheduleServiceImplementation implementation
) : CampusMessage<UpsertAccScheduleRequest, BulkResult<AccommodationSchedule>>(logger)
{
    private readonly ScheduleServiceImplementation _impl = implementation;

    protected override async Task<BulkResult<AccommodationSchedule>> HandleMessage(UpsertAccScheduleRequest request, CancellationToken token)
        => await _impl.UpsertScheduleAsync(request.FileName, request.Name);
}

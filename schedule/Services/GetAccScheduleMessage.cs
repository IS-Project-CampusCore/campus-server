using commons.RequestBase;
using schedule.Implementation;
using schedule.Models;
using scheduleServiceClient;

namespace schedule.Services;

public class GetAccScheduleMessage(
    ILogger<UpsertUniScheduleMessage> logger,
    ScheduleServiceImplementation implementation
) : CampusMessage<GetAccScheduleRequest, AccommodationSchedule>(logger)
{
    private readonly ScheduleServiceImplementation _impl = implementation;

    protected override async Task<AccommodationSchedule> HandleMessage(GetAccScheduleRequest request, CancellationToken token)
        => await _impl.GetAccommodationScheduleAsync(request.Name);
}

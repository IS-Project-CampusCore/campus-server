using commons.RequestBase;
using grades.Implementation;
using gradesServiceClient;
using grades.Models;

namespace grades.Services;

public class GetGradesMessage(
ILogger<GetGradesMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<GetGradesRequest, List<Grade>>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<List<Grade>> HandleMessage(GetGradesRequest request, CancellationToken token) =>
        await _impl.GetGradesAsync(request.StudentId);
}
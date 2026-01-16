using commons.RequestBase;
using grades.Implementation;
using gradesServiceClient;

namespace grades.Services;

public class EnrollMessage(
ILogger<EnrollMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<EnrollRequest>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(EnrollRequest request, CancellationToken token) =>
        await _impl.EnrollToCourseAsync(request.CourseKey, request.StudentId);
}

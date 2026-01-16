using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class GetCourseByNameMessage(
ILogger<GetCourseByNameMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<GetCourseByNameRequest, Course>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<Course> HandleMessage(GetCourseByNameRequest request, CancellationToken token) =>
        await _impl.GetCourseByNameAsync(request.Name);
}

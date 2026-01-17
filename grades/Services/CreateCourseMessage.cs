using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class CreateCourseMessage(
ILogger<CreateCourseMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<CreateCourseRequest, Course>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<Course> HandleMessage(CreateCourseRequest request, CancellationToken token) =>
        await _impl.CreateCourseAsync(request.Name, request.ProfessorId, request.Year);
}
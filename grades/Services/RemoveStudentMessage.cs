using commons.RequestBase;
using grades.Implementation;
using gradesServiceClient;

namespace grades.Services;

public class RemoveStudentMessage(
ILogger<RemoveStudentMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<RemoveStudentRequest>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(RemoveStudentRequest request, CancellationToken token) =>
        await _impl.RemoveStudentAsync(request.CourseId, request.ProfessorId, request.StudentId);
}

using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class RemoveGradeMessage(
ILogger<RemoveGradeMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<RemoveGradeRequest>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(RemoveGradeRequest request, CancellationToken token) =>
        await _impl.RemoveGradeAsync(request.CourseId, request.ProfessorId, request.StudentId);
}
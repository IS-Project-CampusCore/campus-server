using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class AddStudentMessage(
ILogger<AddStudentMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<AddStudentRequest>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(AddStudentRequest request, CancellationToken token) =>
        await _impl.AddStudentAsync(request.CourseId, request.ProfessorId, request.StudentId);
}

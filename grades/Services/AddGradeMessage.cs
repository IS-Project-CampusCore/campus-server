using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class AddGradeMessage(
ILogger<AddGradeMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<AddGradeRequest, Grade>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<Grade> HandleMessage(AddGradeRequest request, CancellationToken token) =>
        await _impl.AddGradeAsync(request.CourseId, request.ProfessorId, request.StudentId, request.Grade);
}
using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class UpdateGradeMessage(
ILogger<UpdateGradeMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<UpdateGradeRequest, Grade>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<Grade> HandleMessage(UpdateGradeRequest request, CancellationToken token) =>
        await _impl.UpdateGradeAsync(request.CourseId, request.ProfessorId, request.StudentId, request.Grade);
}
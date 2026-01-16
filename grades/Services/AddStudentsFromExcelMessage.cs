using grades.Implementation;
using grades.Models;
using gradesServiceClient;
using commons.RequestBase;

namespace grades.Services;

public class AddStudentsFromExcelMessage(
    ILogger<AddStudentsFromExcelMessage> logger,
    GradesServiceImplementation implementation
) : CampusMessage<AddFromExcelRequest, BulkResult<bool>>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<BulkResult<bool>> HandleMessage(AddFromExcelRequest request, CancellationToken token) =>
        await _impl.BulkAddStudentsAsync(request.CourseId, request.ProfessorId, request.FileName);
}

using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class AddGradesFromExcelMessage(
    ILogger<AddGradesFromExcelMessage> logger,
    GradesServiceImplementation implementation
) : CampusMessage<AddGradesFromExcelRequest, BulkResult<List<Grade>>>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<BulkResult<List<Grade>>> HandleMessage(AddGradesFromExcelRequest request, CancellationToken token) =>
        await _impl.BulkGradesOperationAsync(request.CourseId, request.ProfessorId, request.FileName, true);
}

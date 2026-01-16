using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class UpdateGradesFromExcelMessage(
    ILogger<UpdateGradesFromExcelMessage> logger,
    GradesServiceImplementation implementation
) : CampusMessage<UpdateGradesFromExcelRequest, BulkResult<List<Grade>>>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<BulkResult<List<Grade>>> HandleMessage(UpdateGradesFromExcelRequest request, CancellationToken token) =>
        await _impl.BulkGradesOperationAsync(request.CourseId, request.ProfessorId, request.FileName, false);
}
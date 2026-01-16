using grades.Implementation;
using grades.Models;
using gradesServiceClient;
using commons.RequestBase;

namespace grades.Services;

public class AddStudentsFromExcelMessage(
    ILogger<AddStudentsFromExcelMessage> logger,
    GradesServiceImplementation implementation
) : CampusMessage<AddFromExcelRequest, BulkResult>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<BulkResult> HandleMessage(AddFromExcelRequest request, CancellationToken token) =>
        await _impl.BulkAddStudentsAsync(request.CourseId, request.CourseId, request.FileName);
}

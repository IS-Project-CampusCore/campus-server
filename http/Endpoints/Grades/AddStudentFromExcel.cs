using gradesServiceClient;
using http.Auth;
using commons.Protos;

namespace http.Endpoints.Grades;

public record FileRequest(string CourseId, string FileName);

public class AddStudentFromExcel(ILogger<AddStudentFromExcel> logger) : CampusEndpoint<FileRequest>(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/grades/add-excel");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("professor");
    }

    public override async Task HandleAsync(FileRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName) || string.IsNullOrEmpty(req.CourseId))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new AddFromExcelRequest
        {
            FileName = req.FileName,
            CourseId = req.CourseId,
            ProfessorId = GetUserId()
        };

        MessageResponse grpcResponse = await Client.AddStudentsFromExcelAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

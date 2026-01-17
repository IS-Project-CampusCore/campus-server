using http.Auth;
using gradesServiceClient;
using commons.Protos;

namespace http.Endpoints.Grades;

public record StudentGradeRequest(string CourseId, string StudentId, double? Grade);
public class AddGrade(ILogger<AddGrade> logger) : CampusEndpoint<StudentGradeRequest>(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/grades/add");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("professor");
    }

    public override async Task HandleAsync(StudentGradeRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.CourseId) || string.IsNullOrEmpty(req.StudentId) || req.Grade is null || !req.Grade.HasValue)
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new AddGradeRequest
        {
            CourseId = req.CourseId,
            StudentId = req.StudentId,
            ProfessorId = GetUserId(),
            Grade = req.Grade.Value
        };

        MessageResponse grpcResponse = await Client.AddGradeAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
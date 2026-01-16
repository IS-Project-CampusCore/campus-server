using http.Auth;
using gradesServiceClient;
using commons.Protos;

namespace http.Endpoints.Grades;

public class RemoveGrade(ILogger<RemoveGrade> logger) : CampusEndpoint<StudentApiRequest>(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/grades/remove");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("professor");
    }

    public override async Task HandleAsync(StudentApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.CourseId) || string.IsNullOrEmpty(req.StudentId))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new RemoveGradeRequest
        {
            CourseId = req.CourseId,
            StudentId = req.StudentId,
            ProfessorId = GetUserId(),
        };

        MessageResponse grpcResponse = await Client.RemoveGradeAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
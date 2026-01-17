using http.Auth;
using gradesServiceClient;
using commons.Protos;
using FastEndpoints;

namespace http.Endpoints.Grades;

public class GetCourses(ILogger<Enroll> logger) : CampusEndpoint(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/grades/courses");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("professor", "student", "campus_student");
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var grpcRequest = new GetCoursesRequest
        {
            UserId = GetUserId(),
            IsProfessor = GetUserRole() == "professor"
        };

        MessageResponse grpcResponse = await Client.GetUserCoursesAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
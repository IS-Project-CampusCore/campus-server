using http.Auth;
using gradesServiceClient;
using commons.Protos;

namespace http.Endpoints.Grades;

public class Enroll(ILogger<Enroll> logger) : CampusEndpoint<string>(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/grades/enroll");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("student", "campus_student");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new EnrollRequest
        {
            CourseKey = req,
            StudentId = GetUserId()
        };

        MessageResponse grpcResponse = await Client.EnrollToCourseAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
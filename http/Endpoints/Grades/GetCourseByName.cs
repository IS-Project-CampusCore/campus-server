using http.Auth;
using gradesServiceClient;
using commons.Protos;

namespace http.Endpoints.Grades;

public class GetCourseByName(ILogger<Enroll> logger) : CampusEndpoint<string>(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/grades/course");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("professor", "student", "campus_student");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetCourseByNameRequest
        {
            Name = req,
        };

        MessageResponse grpcResponse = await Client.GetCourseByNameAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
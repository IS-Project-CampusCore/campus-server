using http.Auth;
using gradesServiceClient;
using commons.Protos;
using FastEndpoints;

namespace http.Endpoints.Grades;

public class GetGrades(ILogger<Enroll> logger) : CampusEndpoint(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/grades/grades");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("student", "campus_student");
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var grpcRequest = new GetGradesRequest
        {
            StudentId = GetUserId()
        };

        MessageResponse grpcResponse = await Client.GetGradesAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
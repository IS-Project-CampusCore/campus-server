using http.Auth;
using gradesServiceClient;
using commons.Protos;

namespace http.Endpoints.Grades;

public record CreateCourseApiRequest(string Name, int Year);
public class CreateCourse(ILogger<CreateCourse> logger) : CampusEndpoint<CreateCourseApiRequest>(logger)
{
    public gradesService.gradesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/grades/create");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("professor");
    }

    public override async Task HandleAsync(CreateCourseApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Name))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new CreateCourseRequest
        {
            Name = req.Name,
            ProfessorId = GetUserId(),
            Year = req.Year
        };

        MessageResponse grpcResponse = await Client.CreateCourseAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}

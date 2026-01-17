using commons.RequestBase;
using grades.Implementation;
using grades.Models;
using gradesServiceClient;

namespace grades.Services;

public class GetCoursesMessage(
ILogger<GetCoursesMessage> logger,
GradesServiceImplementation implementation
) : CampusMessage<GetCoursesRequest, List<Course>>(logger)
{
    private readonly GradesServiceImplementation _impl = implementation;

    protected override async Task<List<Course>> HandleMessage(GetCoursesRequest request, CancellationToken token) =>
        await _impl.GetUserCoursesAsync(request.UserId, request.IsProfessor);
}
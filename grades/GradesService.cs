using commons.Protos;
using gradesServiceClient;
using Grpc.Core;
using MediatR;

namespace grades;

public class GradesService(IMediator mediator) : gradesService.gradesServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> CreateCourse(CreateCourseRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> AddStudent(AddStudentRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> RemoveStudent(RemoveStudentRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> EnrollToCourse(EnrollRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> AddStudentsFromExcel(AddFromExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetUserCourses(GetCoursesRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetCourseByName(GetCourseByNameRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> AddGrade(AddGradeRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> UpdateGrade(UpdateGradeRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> RemoveGrade(RemoveGradeRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetGrades(GetGradesRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> AddGradesFromExcel(AddGradesFromExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> UpdateGradesFromExcel(UpdateGradesFromExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}


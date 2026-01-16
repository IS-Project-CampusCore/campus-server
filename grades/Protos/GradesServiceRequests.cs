using commons.RequestBase;

namespace gradesServiceClient;

public partial class CreateCourseRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(ProfessorId) || string.IsNullOrEmpty(Name))
            return "Create Course request is empty";
        return null;
    }
}

public partial class AddStudentRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId) || string.IsNullOrEmpty(StudentId))
            return "Add Student request is empty";
        return null;
    }
}

public partial class RemoveStudentRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId) || string.IsNullOrEmpty(StudentId))
            return "Remove Student request is empty";
        return null;
    }
}

public partial class EnrollRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CourseKey) || string.IsNullOrEmpty(StudentId))
            return "Enroll request is empty";
        return null;
    }
}

public partial class AddFromExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId))
            return "Add Students From Excel request is empty";
        return null;
    }
}

public partial class GetCoursesRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(UserId) ? "Get Courses request is empty" : null;
}

public partial class GetCourseByNameRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Name) ? "Get Course By Name request is empty" : null;
}

public partial class AddGradeRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId) || string.IsNullOrEmpty(StudentId))
            return "Add Grade request is empty";
        return null;
    }
}

public partial class UpdateGradeRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId) || string.IsNullOrEmpty(StudentId))
            return "Update Grade request is empty";
        return null;
    }
}

public partial class RemoveGradeRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId) || string.IsNullOrEmpty(StudentId))
            return "Remove Grade request is empty";
        return null;
    }
}

public partial class GetGradesRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(StudentId))
            return "Get Grades request is empty";
        return null;
    }
}

public partial class AddGradesFromExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId))
            return "Add Grades From Excel request is empty";
        return null;
    }
}

public partial class UpdateGradesFromExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(CourseId) || string.IsNullOrEmpty(ProfessorId))
            return "Add Update From Excel request is empty";
        return null;
    }
}
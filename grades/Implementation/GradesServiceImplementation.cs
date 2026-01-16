using commons.Database;
using commons.RequestBase;
using commons.Tools;
using excelServiceClient;
using grades.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using usersServiceClient;
using static MassTransit.Logging.OperationName;

namespace grades.Implementation;

public interface IGradesService
{
    Task<Course> GetCourseByNameAsync(string courseName);
    Task<Course> CreateCourseAsync(string name, string professorId, int year);
    Task AddStudentAsync(string courseId, string professorId, string studentId, bool hasKey = false);
    Task RemoveStudentAsync(string courseId, string professorId, string studentId);
    Task EnrollToCourseAsync(string courseKey, string studentId);

    Task<List<Course>> GetUserCoursesAsync(string userId, bool isProfessor);
    Task<List<Grade>> GetGradesAsync(string studentId);
    Task<Grade> AddGradeAsync(string courseId, string professorId, string studentId, double value);
    Task<Grade> UpdateGradeAsync(string courseId, string professorId, string studentId, double value);
    Task RemoveGradeAsync(string courseId, string professorId, string studentId);
}

public class GradesServiceImplementation(
    ILogger<GradesServiceImplementation> logger,
    IDatabase database,
    usersService.usersServiceClient usersService,
    excelService.excelServiceClient excelService
) : IGradesService
{
    private readonly ILogger<GradesServiceImplementation> _logger = logger;

    private readonly usersService.usersServiceClient _usersService = usersService;
    private readonly excelService.excelServiceClient _excelService = excelService;

    private readonly AsyncLazy<IDatabaseCollection<Course>> _courses = new(() => GetCourseCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Grade>> _grades = new(() => GetGradeCollection(database));

    public async Task<Course> GetCourseByNameAsync(string courseName)
    {
        var courses = await _courses;
        return await courses.GetOneAsync(c => c.Name == courseName);
    }

    public async Task<Course> CreateCourseAsync(string name, string professorId, int year)
    {
        var courses = await _courses;
        if (await courses.ExistsAsync(c => c.Name == name))
        {
            _logger.LogInformation("Course name already used");
            throw new BadRequestException("Course name already used");
        }

        var userRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = professorId });
        if (!userRes.Success || userRes.Body is null)
        {
            _logger.LogInformation($"User:{professorId} not found");
            throw new BadRequestException("User not found");
        }

        var payload = userRes.Payload;
        string userName = payload.GetString("Name");
        int userRole = payload.GetInt32("Role");

        if (userRole != 4)
        {
            _logger.LogInformation("User does not have 'professor' role");
            throw new ForbiddenException("'Professor' role requier for Create Course");
        }

        string key = GenerateSecureKey();

        var course = new Course
        {
            Name = name,
            ProfessorId = professorId,
            Year = year,
            Key = key,
            StudentIds = []
        };

        _logger.LogInformation($"New Course:{name} created for Professor:{userName} with key:{key}");

        await courses.InsertAsync(course);
        return course;
    }

    public async Task AddStudentAsync(string courseId, string professorId, string studentId, bool hasKey = false)
    {
        var courses = await _courses;

        if (!hasKey && !await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        var course = await courses.GetOneByIdAsync(courseId);

        if (course.StudentIds.Contains(studentId))
        {
            _logger.LogInformation($"Student:{studentId} is already enrolled to Course:{course.Name}");
            throw new BadRequestException("Student already added");
        }

        var userRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = studentId });
        if (!userRes.Success || userRes.Body is null)
        {
            _logger.LogInformation($"User:{studentId} not found");
            throw new NotFoundException("User not found");
        }

        var payload = userRes.Payload;
        string userName = payload.GetString("Name");
        int userRole = payload.GetInt32("Role");

        if (userRole != 3)
        {
            _logger.LogInformation("User does not have 'student' role");
            throw new ForbiddenException("User is requiered to have 'Student' role to be added in a course");
        }

        course.StudentIds.Add(studentId);
        await courses.ReplaceAsync(course);
    }

    public async Task RemoveStudentAsync(string courseId, string professorId, string studentId)
    {
        var courses = await _courses;

        if (!await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        var course = await courses.GetOneByIdAsync(courseId);

        if (!course.StudentIds.Contains(studentId))
        {
            _logger.LogInformation("Student is not enrolled in this course");
            throw new NotFoundException("Student not found");
        }

        course.StudentIds.Remove(studentId);

        await courses.ReplaceAsync(course);
    }

    public async Task EnrollToCourseAsync(string courseKey, string studentId)
    {
        var courses = await _courses;

        var course = await courses.GetOneAsync(c => c.Key == courseKey);
        if (course is null)
        {
            _logger.LogInformation($"Course with Key:{courseKey} was not found");
            throw new NotFoundException("Course not found");
        }

        await AddStudentAsync(course.Id, string.Empty, studentId, true);
    }

    public async Task<List<Course>> GetUserCoursesAsync(string userId, bool isProfessor)
    {
        var courses = await _courses;
        if (isProfessor)
        {
            return await courses.MongoCollection.Find(c => c.ProfessorId == userId).ToListAsync();
        }

        return await courses.MongoCollection.Find(c => c.StudentIds.Contains(userId)).ToListAsync();
    }

    public async Task<List<Grade>> GetGradesAsync(string studentId)
    {
        var grades = await _grades;
        return await grades.MongoCollection.Find(g => g.StudentId == studentId).ToListAsync();
    }

    public async Task<Grade> AddGradeAsync(string courseId, string professorId, string studentId, double value)
    {
        if (value < 1 || value > 10)
        {
            _logger.LogInformation($"Invalid Grade Value:{value}");
            throw new BadRequestException("Grades need to be between 1 and 10");
        }

        var grades = await _grades;
        if (await grades.ExistsAsync(g => g.StudentId == studentId && g.CourseId == courseId))
        {
            _logger.LogInformation($"Student:{studentId} already has a grade to Course:{courseId}");
            throw new BadRequestException("Student is already graded");
        }

        var courses = await _courses;

        if (!await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        var course = await courses.GetOneByIdAsync(courseId);

        if (!course.StudentIds.Contains(studentId))
        {
            _logger.LogInformation($"Student:{studentId} is not enrolled to Course:{courseId}");
            throw new NotFoundException("Student not found");
        }

        var userRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = studentId });
        if (!userRes.Success || userRes.Body is null)
        {
            _logger.LogInformation($"User:{studentId} not found");
            throw new NotFoundException("User not found");
        }

        Grade grade = new Grade
        {
            StudentId = studentId,
            CourseId = courseId,
            Value = value
        };

        await grades.InsertAsync(grade);
        return grade;
    }

    public async Task<Grade> UpdateGradeAsync(string courseId, string professorId, string studentId, double value)
    {
        if (!await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        if (value < 1 || value > 10)
        {
            _logger.LogInformation($"Invalid Grade Value:{value}");
            throw new BadRequestException("Grades need to be between 1 and 10");
        }

        var grades = await _grades;

        var grade = await grades.GetOneAsync(g => g.CourseId == courseId && g.StudentId == studentId);
        if (grade is null)
        {
            _logger.LogInformation($"Student:{studentId} does not have a grade for Course{courseId}");
            throw new NotFoundException("Student was not graded");
        }

        grade.Value = value;

        await grades.ReplaceAsync(grade);
        return grade;
    }

    public async Task RemoveGradeAsync(string courseId, string professorId, string studentId)
    {
        if (!await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        var grades = await _grades;

        var grade = await grades.GetOneAsync(g => g.CourseId == courseId && g.StudentId == studentId);
        if (grade is null)
        {
            _logger.LogInformation($"Student:{studentId} does not have a grade for Course{courseId}");
            throw new NotFoundException("Student was not graded");
        }

        await grades.DeleteWithIdAsync(grade.Id);
    }

    public async Task<BulkResult<bool>> BulkAddStudentsAsync(string courseId, string professorId, string fileName)
    {
        if (!await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        List<string> headers;
        List<List<(string CellType, object? Value)>> rows;

        (headers, rows) = await GetExcelData(fileName, ["string"]);

        if (headers.Except(["Email"]).Any())
        {
            _logger.LogError("Excel template does not match");
            throw new BadRequestException("Excel template does not match");
        }

        int total = 0;
        int success = 0;
        int skipped = 0;
        List<string> errors = [];
        foreach (var row in rows)
        {
            total++;

            object? email = row.ElementAt(0).Value;
            if (email is not string emailStr || string.IsNullOrEmpty(emailStr))
            {
                skipped++;
                errors.Add($"Missing email at row:{total}");
                continue;
            }

            var userRes = await _usersService.GetUserByEmailAsync( new UserEmailRequest {  Email = emailStr });
            if (!userRes.Success || userRes.Body is null)
            {
                skipped++;
                errors.Add($"Invalid email at row:{total}");
                continue;
            }

            var payload = userRes.Payload;
            string userId = payload.GetString("Id");

            try
            {
                await AddStudentAsync(courseId, professorId, userId);
                success++;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                errors.Add($"{ex.Message} at row:{total}");
                skipped++;
            }
        }

        return new BulkResult<bool>
        {
            TotalCount = total,
            SuccessCount = success,
            SkipedCount = skipped,
            Errors = errors,
            Result = success == total
        };
    }

    public async Task<BulkResult<List<Grade>>> BulkGradesOperationAsync(string courseId, string professorId, string fileName, bool isInsert = false)
    {
        if (!await IsCourseProfessorOrThrow(courseId, professorId))
        {
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        List<string> headers;
        List<List<(string CellType, object? Value)>> rows;

        (headers, rows) = await GetExcelData(fileName, ["string", "double"]);

        if (headers.Except(["Email", "Grade"]).Any())
        {
            _logger.LogError("Excel template does not match");
            throw new BadRequestException("Excel template does not match");
        }

        int total = 0;
        int success = 0;
        int skipped = 0;
        List<string> errors = [];
        List<Grade> grades = [];
        foreach (var row in rows)
        {
            total++;

            object? email = row.ElementAt(0).Value;
            object? grade = row.ElementAt(1).Value;

            if (email is not string emailStr || grade is not double gradeDou)
            {
                skipped++;
                errors.Add($"Missing data at row:{total}");
                continue;
            }

            var userRes = await _usersService.GetUserByEmailAsync(new UserEmailRequest { Email = emailStr });
            if (!userRes.Success || userRes.Body is null)
            {
                skipped++;
                errors.Add($"Invalid email at row:{total}");
                continue;
            }

            var payload = userRes.Payload;
            string userId = payload.GetString("Id");

            try
            {
                Grade resGrade =  isInsert ? await AddGradeAsync(courseId, professorId, userId, gradeDou) : await UpdateGradeAsync(courseId, professorId, userId, gradeDou);
                grades.Add(resGrade);
                success++;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                errors.Add($"{ex.Message} at row:{total}");
                skipped++;
            }
        }

        return new BulkResult<List<Grade>>
        {
            TotalCount = total,
            SuccessCount = success,
            SkipedCount = skipped,
            Errors = errors,
            Result = grades
        };
    }

    private async Task<bool> IsCourseProfessorOrThrow(string courseId, string professorId)
    {
        var courses = await _courses;

        var course = await courses.GetOneByIdAsync(courseId);
        if (course is null)
        {
            _logger.LogError($"Course:{courseId} was not found");
            throw new NotFoundException("Course not found");
        }

        if (course.ProfessorId != professorId)
        {
            _logger.LogInformation($"Profesor:{professorId} cannot modify students/grades to Course:{course.Name}, Professor:{course.ProfessorId}");
            throw new ForbiddenException("Just the course professor has access to this course");
        }

        return true;
    }

    private async Task<(List<string> Headers, List<List<(string CellType, object? Value)>> Rows)> GetExcelData(string fileName, string[] types)
    {
        if (fileName is null)
            throw new BadRequestException("No file selected");

        var request = new ParseExcelRequest
        {
            FileName = fileName
        };

        request.CellTypes.AddRange(types);

        var response = await _excelService.ParseExcelAsync(request);
        if (!response.Success)
            throw new InternalErrorException(response.Errors);

        if (string.IsNullOrEmpty(response.Body))
            throw new BadRequestException("Empty excel file");

        var payload = response.Payload;
        var errors = payload.GetArray("Errors").IterateStrings().ToList();

        if (errors is not null && errors.Any())
        {
            string aggregatedErrors = string.Join("\n", errors);
            _logger.LogError($"Excel parsing failed with {errors.Count()} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        var headers = payload.GetArray("Headers").IterateStrings().ToList();
        var rows = payload.GetArray("Rows")
            .Iterate()
            .Select(rowBody => rowBody.Iterate()
            .Select(cellBody => (
                CellType : cellBody.GetString("CellType"),
                Value : ExtractValue(cellBody)
            ))
            .ToList())
            .ToList();

        if (headers is null || !headers.Any() || rows is null || !rows.Any())
        {
            _logger.LogError("Excel could not be parsed");
            throw new InternalErrorException("Excel could not be parsed");
        }

        return (headers, rows);
    }
    private object? ExtractValue(commons.Protos.MessageBody cell)
    {
        if (cell.TryGetString("Value") is string s)
            return s;

        if (cell.TryGetDouble("Value") is double b)
            return b;

        return null;
    }

    private string GenerateSecureKey(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var result = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            int index = RandomNumberGenerator.GetInt32(chars.Length);
            result.Append(chars[index]);
        }

        return result.ToString();
    }

    internal static async Task<IDatabaseCollection<Course>> GetCourseCollection(IDatabase database)
    {
        var collection = database.GetCollection<Course>();

        var nameIndex = Builders<Course>.IndexKeys.Ascending(c => c.Name);
        var professorIndex = Builders<Course>.IndexKeys.Ascending(c => c.ProfessorId);
        var keyIndex = Builders<Course>.IndexKeys.Ascending(c => c.Key);

        var index1 = new CreateIndexModel<Course>(nameIndex, new CreateIndexOptions
        {
            Name = "nameIndex"
        });

        var index2 = new CreateIndexModel<Course>(professorIndex, new CreateIndexOptions
        {
            Name = "professorIndex"
        });

        var index3 = new CreateIndexModel<Course>(keyIndex, new CreateIndexOptions
        {
            Name = "keyIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync([index1, index2, index3]);
        return collection;
    }

    internal static async Task<IDatabaseCollection<Grade>> GetGradeCollection(IDatabase database)
    {
        var collection = database.GetCollection<Grade>();

        var studentId = Builders<Grade>.IndexKeys.Ascending(g => g.StudentId);
        var courseId = Builders<Grade>.IndexKeys.Ascending(g => g.CourseId);

        var index1 = new CreateIndexModel<Grade>(studentId, new CreateIndexOptions
        {
            Name = "studentIdIndex"
        });

        var index2 = new CreateIndexModel<Grade>(courseId, new CreateIndexOptions
        {
            Name = "courseIdIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync([index1, index2]);
        return collection;
    }
}

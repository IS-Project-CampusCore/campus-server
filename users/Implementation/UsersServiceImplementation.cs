using BCrypt.Net;
using commons;
using commons.Database;
using commons.RequestBase;
using commons.Tools;
using emailServiceClient;
using excelServiceClient;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using users.Model;
using users.Utils;
using usersServiceClient;

namespace users.Implementation;

public interface IUsersServiceImplementation
{
    public Task<User?> GetUserById(string id);
    Task<List<User>> GetAllUsers();
    Task<List<User>> GetUsersByRole(UserType role);
    Task<List<User>> GetUsersByUniversity(string university);
    public Task<User?> GetUserByEmail(string email);
    public Task<User?> RegisterUser(string email, string name, UserType role, string? university, int? year, int? group, string? major, string? department, string? title);
    public Task<UserWithJwt> AuthUser(string email, string password);
    public Task<UserWithJwt> Verify(string email, string password, string verifyCode);
    public Task<BulkResponse> BulkRegisterAsync(string fileName);
    public Task ResendVerifyCode(string email);
    Task DeleteAccount(string userId);
    Task ResetPassword(string email);
    Task<User?> UpdateUserAsync(string email, string? name, UserType? role, string? university, int? year, int? group, string? major, string? dormitory, string? room, string? department, string? title);
    public Task<BulkResponse> BulkUpdateAsync(string fileName);
}

public class UsersServiceImplementation(
    ILogger<UsersServiceImplementation> logger,
    IConfiguration config,
    IDatabase database,
    emailService.emailServiceClient emailService,
    excelService.excelServiceClient excelService
    ) : IUsersServiceImplementation
{
    private readonly ILogger<UsersServiceImplementation> _logger = logger;
    private readonly IConfiguration _config = config;
    private readonly emailService.emailServiceClient _emailService = emailService;
    private readonly excelService.excelServiceClient _excelService = excelService;

    private readonly AsyncLazy<IDatabaseCollection<User>> _usersCollection = new(() => GetUserCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<VerifyCode>> _verifyCollection = new(() => GetVerifyCollection(database));

    private static string[] s_userInfoTypes = { "string", "string", "string", "string?", "double?", "double?", "string?", "string?", "string?" };

    public async Task<List<User>> GetAllUsers()
    {
        var users = await _usersCollection;
        return await users.MongoCollection.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetUserById(string id)
    {
        var users = await _usersCollection;
        return await users.GetOneByIdAsync(id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var users = await _usersCollection;
        return await users.GetOneAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetUsersByRole(UserType role)
    {
        var users = await _usersCollection;
        return await users.MongoCollection.Find(u => u.Role == role).ToListAsync();
    }

    public async Task<List<User>> GetUsersByUniversity(string university)
    {
        var users = await _usersCollection;

        var filter = Builders<User>.Filter.OfType<Communicator>(c => c.University == university);
        return await users.MongoCollection.Find(filter).ToListAsync();
    }

    public async Task<UserWithJwt> AuthUser(string email, string password)
    {
        if (!IsValidEmail(email) || !IsValidPassword(password))
        {
            throw new BadRequestException("Email or password incorrect");
        }

        User? user = await FindByEmailOrDefault(email);

        if (user is null || !VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogError($"No user found for Email:{email}");
            throw new BadRequestException("Incorrect Email or Password");
        }

        if (!user.IsVerified)
        {
            _logger.LogError($"User with Email:{email} is not verified");
            throw new BadRequestException($"The user:{user.Name} is not verified");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await UpdateUserAsync(user);

        _logger.LogInformation($"User:{user.Id} has been authenticated");
        return new UserWithJwt(user, GenerateJwtToken(user.Id, user.Email, user.Name, user.Role));
    }

    public async Task<User?> RegisterUser(string email, string name, UserType role, string? university, int? year, int? group, string? major, string? department, string? title)
    {
        User? user = NewUserFromData(email, name, role, university, year, group, major, null, null, department, title);
        if (user is null)
        {
            _logger.LogError("Invalid user data");
            throw new BadRequestException("Invalid user data");
        }
        return await RegisterUser(user);
    }

    private async Task<User?> RegisterUser(User newUser)
    {
        if (!IsValidEmail(newUser.Email))
        {
            throw new BadRequestException("Invalid email address");
        }

        var users = await _usersCollection;

        if (await users.ExistsAsync(u => u.Email == newUser.Email))
        {
            _logger.LogError($"Email:{newUser.Email} is already used");
            throw new BadRequestException($"Email:{newUser.Email} is already used");
        }

        await users.InsertAsync(newUser);

        string verifyCode = GenerateVerifyCode();
        _logger.LogInformation($"Verify Code:{verifyCode} generated for email: {newUser.Email}");

        await StoreVerifyCode(newUser.Id, verifyCode);

        await SendVerifyEmail(newUser.Email, newUser.Name, verifyCode);

        _logger.LogInformation($"User:{newUser.Email} has been registered");
        return newUser;
    }

    public async Task DeleteAccount(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new BadRequestException("User ID cannot be empty.");
        }

        var db = await _usersCollection;

        bool exists = await db.ExistsAsync(u => u.Id == userId);

        if (!exists)
        {
            throw new NotFoundException($"User with id {userId} not found.");
        }

        await db.DeleteWithIdAsync(userId);

        _logger.LogInformation($"User with ID:{userId} has been deleted.");
    }

    public async Task<UserWithJwt> Verify(string email, string password, string verifyCode)
    {
        if (!IsValidEmail(email) || !IsValidPassword(password))
        {
            throw new BadRequestException("Email or password incorrect");
        }

        User? user = await FindByEmailOrDefault(email);
        if (user is null)
        {
            _logger.LogError($"No user found for Email:{email}");
            throw new BadRequestException("Incorrect Email");
        }

        bool hasCode = await HasVerifyCode(user.Id);
        if (!hasCode || user.IsVerified)
        {
            _logger.LogError($"User already verified or no code sent");
            throw new BadRequestException("User already verified or code invalid");
        }

        string? code = await GetVerificationCode(user.Id);
        if (code is null || code != verifyCode)
        {
            _logger.LogError($"Verify code did not match");
            throw new BadRequestException($"Verify code did not match");
        }

        try
        {
            await RemoveVerificationCode(user.Id);

            string passwordHash = HashPassword(password);

            user.PasswordHash = passwordHash;
            user.IsVerified = true;

            await UpdateUserAsync(user);

            return new UserWithJwt(user, GenerateJwtToken(user.Id, email, user.Name, user.Role));
        }
        catch (BadRequestException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task ResendVerifyCode(string email)
    {
        if (!IsValidEmail(email))
        {
            throw new BadRequestException("Invalid email address");
        }

        User? user = await FindByEmailOrDefault(email);

        if (user is null)
        {
            _logger.LogError($"ResendCode failed: User with Email:{email} not found.");
            throw new BadRequestException("User not found.");
        }

        if (user.IsVerified)
        {
            _logger.LogWarning($"ResendCode skipped: User:{email} is already verified.");
            throw new BadRequestException("User is already verified.");
        }

        string codeToSend;
        bool hasCode = await HasVerifyCode(user.Id);

        if (hasCode)
        {
            string? existingCode = await GetVerificationCode(user.Id);
            if (string.IsNullOrEmpty(existingCode))
            {
                throw new InternalErrorException("System indicates code exists but none found.");
            }

            codeToSend = existingCode;
            _logger.LogInformation($"Resending existing code for User:{email}");
        }
        else
        {
            codeToSend = GenerateVerifyCode();
            await StoreVerifyCode(user.Id, codeToSend);
            _logger.LogInformation($"Generated new code for User:{email} as none existed.");
        }

        await SendVerifyEmail(user.Email, user.Name, codeToSend);
    }

    public async Task<BulkResponse> BulkRegisterAsync(string fileName)
    {
        if (fileName is null) throw new BadRequestException("No file selected");

        var request = new ParseExcelRequest
        {
            FileName = fileName
        };

        request.CellTypes.AddRange(s_userInfoTypes);

        var response = await _excelService.ParseExcelAsync(request);

        if (!response.Success)
            throw new InternalErrorException(response.Errors);

        if (string.IsNullOrEmpty(response.Body)) 
            throw new BadRequestException("Empty excel file");

        var payload = response.Payload;
        IEnumerable<string>? errors = payload.TryGetArray("Errors")?.IterateStrings();

        if (errors is not null && errors.Any())
        {
            string aggregatedErrors = string.Join("\n", errors);
            _logger.LogError($"Excel parsing failed with {errors.Count()} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        ExcelData? data = response.GetPayload<ExcelData>();
        if (data is null)
        {
            _logger.LogError("Excel data was corrupted");
            throw new InternalErrorException("Corrupted excel data");
        }

        List<User> users;
        try
        {
           (users, errors) = ExcelToUserModels.ConvertToUserModels(data);
        }
        catch (ServiceMessageException ex)
        {
            _logger.LogError(ex, $"Convert to User model failed with Exception:{ex.GetType().Name}, Msg:{ex.Message}");
            throw new BadRequestException(ex.Message);
        }

        if (errors is not null && errors.Any())
        {
            string aggregatedErrors = string.Join("\n", errors);
            _logger.LogError($"Excel parsing failed with {errors.Count()} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        int total = users.Count;
        int registered = 0;
        int skiped = 0;
        List<PerUserResult> results = [];

        foreach (var user in users)
        {
            try
            {
                User? newUser = await RegisterUser(user);
                results.Add(new PerUserResult(newUser!.Email));
                registered++;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Request(Email:{user.Email}) exit with Error:{ex.Message}");
                results.Add(new PerUserResult(user.Email, ex.Message));
                skiped++;
            }
        }
        return new BulkResponse
        {
            TotalCount = total,
            SuccessCount = registered,
            SkipedCount = skiped,
            Results = results
        };
    }

    public async Task<BulkResponse> BulkUpdateAsync(string fileName)
    {
        if (fileName is null) 
            throw new BadRequestException("No file selected");

        var request = new ParseExcelRequest
        {
            FileName = fileName
        };

        request.CellTypes.AddRange(s_userInfoTypes);

        var response = await _excelService.ParseExcelAsync(request);

        if (!response.Success)
            throw new InternalErrorException(response.Errors);

        if (string.IsNullOrEmpty(response.Body))
            throw new BadRequestException("Empty excel file");

        var payload = response.Payload;
        IEnumerable<string>? errors = payload.TryGetArray("Errors")?.IterateStrings();

        if (errors is not null && errors.Any())
        {
            string aggregatedErrors = string.Join("\n", errors);
            _logger.LogError($"Excel parsing failed with {errors.Count()} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        ExcelData? data = response.GetPayload<ExcelData>();
        if (data is null)
        {
            _logger.LogError("Excel data was corrupted");
            throw new InternalErrorException("Corrupted excel data");
        }

        List<User> users;
        try
        {
            (users, errors) = ExcelToUserModels.ConvertToUserModels(data, false, false);
        }
        catch (ServiceMessageException ex)
        {
            _logger.LogError(ex, $"Convert to User model failed with Exception:{ex.GetType().Name}, Msg:{ex.Message}");
            throw new BadRequestException(ex.Message);
        }

        if (errors is not null && errors.Any())
        {
            string aggregatedErrors = string.Join("\n", errors);
            _logger.LogError($"Excel parsing failed with {errors.Count()} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        int total = users.Count;
        int registered = 0;
        int skiped = 0;
        List<PerUserResult> results = [];

        foreach (var user in users)
        {
            try
            {
                User? newUser = await UpdateUserAsync(user.Email, user);
                results.Add(new PerUserResult(newUser!.Email));
                registered++;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Request(Email:{user.Email}) exit with Error:{ex.Message}");
                results.Add(new PerUserResult(user.Email, ex.Message));
                skiped++;
            }
        }
        return new BulkResponse
        {
            TotalCount = total,
            SuccessCount = registered,
            SkipedCount = skiped,
            Results = results
        };
    }

    public async Task ResetPassword(string email)
    {
        var db = await _usersCollection;

        var user = await db.GetOneAsync(u => u.Email == email);

        if (user is null)
        {
            throw new BadRequestException($"User with email {email} does not exist.");
        }
        await db.UpdateAsync(u => u.Email == email, u => u.IsVerified, false);

        string verfiyCode = GenerateVerifyCode();
        _logger.LogInformation($"Reset Password Code:{verfiyCode} generated for email: {email}");

        await StoreVerifyCode(user.Id, verfiyCode);

        await SendVerifyEmail(user.Email, user.Name, verfiyCode);
    }

    public async Task<User?> UpdateUserAsync(string email, string? name, UserType? role, string? university, int? year, int? group, string? major, string? dormitory, string? room, string? department, string? title)
    {
        var users = await _usersCollection;
        User? user = await users.GetOneAsync(u => u.Email == email);
        if (user is null)
        {
            _logger.LogError($"User with Email:{email} was not found.");
            throw new NotFoundException("User not found");
        }

        name ??= user.Name;
        role ??= user.Role;

        if (user is Communicator communicator)
        {
            university ??= communicator.University;
        }

        if (user is Professor professor)
        {
            department ??= professor.Department;
            title ??= professor.Title;
        }

        if (user is Student student)
        {
            year ??= student.Year;
            group ??= student.Group;
            major ??= student.Major;
        }

        if(user is CampusStudent campusStudent)
        {
            dormitory ??= campusStudent.Dormitory;
            room ??= campusStudent.Room;
        }

        User? updatedUser = NewUserFromData(email, name, role, university, year, group, major, dormitory, room, department, title);
        if (updatedUser is null)
        {
            _logger.LogError("Could not update User");
            throw new InternalErrorException("Could not update user");
        }

        return await UpdateUserAsync(email, updatedUser);
    }

    private async Task<User> UpdateUserAsync(string email, User updatedUser)
    {
        var users = await _usersCollection;
        User? user = await users.GetOneAsync(u => u.Email == email);
        if (user is null)
        {
            _logger.LogError($"User was not found.");
            throw new NotFoundException("User not found");
        }

        updatedUser.Id = user.Id;
        updatedUser.IsVerified = user.IsVerified;
        updatedUser.PasswordHash = user.PasswordHash;
        updatedUser.CreatedAt = user.CreatedAt;
        updatedUser.LastLoginAt = user.LastLoginAt;

        await users.ReplaceAsync(updatedUser);
        return updatedUser;
    }

    private async Task<User> UpdateUserAsync(User userToUpdate)
    {
        var db = await _usersCollection;
        if (!await db.ExistsAsync(u => u.Id == userToUpdate.Id))
        {
            throw new NotFoundException($"User not found");
        }
        await db.ReplaceAsync(userToUpdate);
        return userToUpdate;
    }

    private bool IsValidEmail(string email)
    {

        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
    }

    private bool IsValidPassword(string password)
    {
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[{\]};:'"",<.>/?\\|`~]).{8,}$";
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, passwordPattern);
    }

    private async Task<User?> FindByEmailOrDefault(string email)
    {
        var db = await _usersCollection;
        return await db.GetOneAsync(x => x.Email == email);
    }

    private string GenerateVerifyCode() => RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");

    private async Task StoreVerifyCode(string userId, string code)
    {
        var db = await _verifyCollection;

        if (await db.ExistsAsync(v => v.UserId == userId))
        {
            var existing = await db.GetOneAsync(v => v.UserId == userId);
            if (existing is not null) 
                await db.DeleteWithIdAsync(existing.Id);
        }

        var verifyEntry = new VerifyCode
        {
            UserId = userId,
            Code = code,
            CreatedAt = DateTime.UtcNow
        };

        await db.InsertAsync(verifyEntry);
    }

    private async Task<bool> HasVerifyCode(string userId)
    {
        var db = await _verifyCollection;
        return await db.ExistsAsync(v => v.UserId == userId);
    }

    private async Task<string?> GetVerificationCode(string userId)
    {
        var db = await _verifyCollection;
        var entry = await db.GetOneAsync(v => v.UserId == userId);
        return entry?.Code;
    }

    private async Task RemoveVerificationCode(string userId)
    {
        var db = await _verifyCollection;
        var entry = await db.GetOneAsync(v => v.UserId == userId);
        if (entry != null)
        {
            await db.DeleteWithIdAsync(entry.Id);
        }
    }

    private async Task SendVerifyEmail(string email, string name, string code)
    {
        string templateDataString = JsonSerializer.Serialize(new { Name = name, Code = code });
        var response = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            ToEmail = email,
            ToName = name,
            TemplateName = "Welcome",
            TemplateData = templateDataString
        });

        if (!response.Success)
        {
            _logger.LogError($"SendEmail failed: {response.Errors}");
            throw new InternalErrorException("Could not send reset email.");
        }
    }

    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, 13);

    private bool VerifyPassword(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);

    private static User? NewUserFromData(string? email, string? name, UserType? role, string? university, int? year, int? group, string? major, string? dormitory, string? room, string? department, string? title)
    {
        switch (role)
        {
            case UserType.CAMPUS_STUDENT:
                return NewCampusStudentFromData(email, name, university, year, group, major, dormitory, room);
            case UserType.STUDENT:
                return NewStudentFromData(email, name, university, year, group, major);
            case UserType.PROFESSOR:
                return NewProfessorFromData(email, name, university, department, title);
            case UserType.MANAGEMENT:
                return NewManagementFromData(email, name);
            case UserType.ADMIN:
                return NewAdminFromData(email, name);
            case UserType.GUEST:
                return NewUserFromData(email, name);
            default:
                return null;
        }
    }

    private static CampusStudent? NewCampusStudentFromData(string? email, string? name, string? university, int? year, int? group, string? major, string? dormitory, string? room)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            return null;

        if (string.IsNullOrEmpty(university) || !year.HasValue || !group.HasValue || string.IsNullOrEmpty(major))
            return null;

        return new CampusStudent
        {
            Email = email,
            Name = name,
            Role = UserType.CAMPUS_STUDENT,
            University = university,
            Year = year.Value,
            Group = group.Value,
            Major = major,
            Dormitory = dormitory,
            Room = room
        };
    }

    private static Student? NewStudentFromData(string? email, string? name, string? university, int? year, int? group, string? major)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            return null;

        if (string.IsNullOrEmpty(university) || !year.HasValue || !group.HasValue || string.IsNullOrEmpty(major))
            return null;

        return new Student
        {
            Email = email,
            Name = name,
            Role = UserType.STUDENT,
            University = university,
            Year = year.Value,
            Group = group.Value,
            Major = major
        };
    }

    private static Professor? NewProfessorFromData(string? email, string? name, string? university, string? department, string? title)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            return null;

        if (string.IsNullOrEmpty(university) || string.IsNullOrEmpty(department) || string.IsNullOrEmpty(title))
            return null;

        return new Professor
        {
            Email = email,
            Name = name,
            Role = UserType.PROFESSOR,
            University = university,
            Department = department,
            Title = title
        };
    }

    private static Management? NewManagementFromData(string? email, string? name)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            return null;

        return new Management
        {
            Email = email,
            Name = name,
            Role = UserType.MANAGEMENT
        };
    }
    private static Admin? NewAdminFromData(string? email, string? name)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            return null;

        return new Admin
        {
            Email = email,
            Name = name,
            Role = UserType.ADMIN
        };
    }
    private static User? NewUserFromData(string? email, string? name)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            return null;

        return new Admin
        {
            Email = email,
            Name = name,
            Role = UserType.GUEST
        };
    }

    private string GenerateJwtToken(string userId, string userEmail, string userName, UserType role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["SecretKey"] ?? "a_very_secret_key_that_must_be_long_and_complex");
        var claims = new[]
        {
            new Claim(UserJwtExtensions.IdClaim, userId),
            new Claim(UserJwtExtensions.EmailClaim, userEmail),
            new Claim(UserJwtExtensions.NameClaim, userName),
            new Claim(UserJwtExtensions.RoleClaim, role.ToString().ToLowerInvariant()),
            new Claim(UserJwtExtensions.IsVerifiedClaim, "true")
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    internal static async Task<IDatabaseCollection<User>> GetUserCollection(IDatabase database)
    {
        var collection = database.GetCollection<User>();
        var roleIndex = Builders<User>.IndexKeys.Ascending(u => u.Role);
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<User>(roleIndex, new CreateIndexOptions { Name = "UserRoleIndex" }),
            new CreateIndexModel<User>(emailIndex, new CreateIndexOptions { Name = "UserEmailIndex", Unique = true })
        ]);
        return collection;
    }

    internal static async Task<IDatabaseCollection<VerifyCode>> GetVerifyCollection(IDatabase database)
    {
        var collection = database.GetCollection<VerifyCode>();
        var userIndex = Builders<VerifyCode>.IndexKeys.Ascending(v => v.UserId);

        await collection.MongoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<VerifyCode>(userIndex, new CreateIndexOptions { Name = "VerifyUserIdIndex" })
        );

        return collection;
    }
}
using BCrypt.Net;
using commons;
using commons.Database;
using commons.RequestBase;
using commons.Tools;
using emailServiceClient;
using excelServiceClient;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
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
    public Task<User?> RegisterUser(string email, string name, UserType role);
    public Task<UserWithJwt> AuthUser(string email, string password);
    public Task<UserWithJwt> Verify(string email, string password, string verifyCode);
    public Task<List<User?>> RegisterUsersFromExcel(string fileName);
    public Task ResendVerifyCode(string email);
    Task DeleteAccount(string userId);
    Task ResetPassword(string email);
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

    public async Task<User?> GetUserById(string id)
    {
        var db = await _usersCollection;
        return await db.GetOneByIdAsync(id);
    }

    public async Task<UserWithJwt> AuthUser(string email, string password)
    {
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
        await UpdateUser(user);

        _logger.LogInformation($"User:{user.Id} has been authenticated");
        return new UserWithJwt(user, GenerateJwtToken(user.Id, user.Email, user.Role));
    }

    public async Task<User?> RegisterUser(string email, string name, UserType role)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Email cannot be empty.");
        }

        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email, emailPattern))
        {
            throw new BadRequestException("Please enter a valid email address.");
        }

        var db = await _usersCollection;

        if (await db.ExistsAsync(u => u.Email == email))
        {
            _logger.LogError($"Email:{email} is already used");
            throw new BadRequestException($"Email:{email} is already used");
        }

        User newUser = new User
        {
            Email = email,
            Name = name,
            Role = role,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await db.InsertAsync(newUser);

        int verifyCodeNumber = RandomNumberGenerator.GetInt32(0, 10000);
        var verfyCode = verifyCodeNumber.ToString("D4");

        _logger.LogInformation($"Verify Code:{verfyCode} generated for email: {email}");

        await StoreVerifyCode(newUser.Id, verfyCode);

        string templateDataString = JsonSerializer.Serialize(new { Name = name, Code = verfyCode });
        var response = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            ToEmail = email,
            ToName = name,
            TemplateName = "Welcome",
            TemplateData = templateDataString
        });

        if (!response.Success)
        {
            _logger.LogError($"SendEmail has failed with Code:{response.Code} and Error:{response.Errors}");
            throw new InternalErrorException(response.Errors);
        }

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
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[{\]};:'"",<.>/?\\|`~]).{8,}$";
        if (string.IsNullOrWhiteSpace(password) || !Regex.IsMatch(password, passwordPattern))
        {
            throw new BadRequestException("Password does not meet security requirements.");
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

            await UpdateUser(user);

            return new UserWithJwt(user, GenerateJwtToken(user.Id, email, user.Role));
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
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Email cannot be empty.");
        }

        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email, emailPattern))
        {
            throw new BadRequestException("Please enter a valid email address.");
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
            int verifyCodeNumber = RandomNumberGenerator.GetInt32(0, 10000);
            codeToSend = verifyCodeNumber.ToString("D4");
            await StoreVerifyCode(user.Id, codeToSend);
            _logger.LogInformation($"Generated new code for User:{email} as none existed.");
        }

        string templateDataString = JsonSerializer.Serialize(new { Name = user.Name, Code = codeToSend });

        var response = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            ToEmail = user.Email,
            ToName = user.Name,
            TemplateName = "Welcome",
            TemplateData = templateDataString
        });

        if (!response.Success)
        {
            _logger.LogError($"Resend email failed with Code:{response.Code} and Error:{response.Errors}");
            throw new InternalErrorException(response.Errors);
        }
    }

    public async Task<List<User?>> RegisterUsersFromExcel(string fileName)
    {
        if (fileName is null) throw new BadRequestException("No file selected");

        var request = new ParseExcelRequest
        {
            FileName = fileName
        };

        request.CellTypes.Add("String");
        request.CellTypes.Add("String");
        request.CellTypes.Add("String");

        var response = await _excelService.ParseExcelAsync(request);

        if (!response.Success) throw new InternalErrorException(response.Errors);
        if (string.IsNullOrEmpty(response.Body)) throw new BadRequestException("Empty excel file");

        ExcelData data = response.GetPayload<ExcelData>()!;

        if (data.Errors != null && data.Errors.Count > 0)
        {
            string aggregatedErrors = string.Join("\n", data.Errors);
            _logger.LogError($"Excel parsing failed with {data.Errors.Count} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        List<RegisterRequest> requests = ExcelToRegisterRequest.ConvertToRegisterRequest(data);

        List<User?> users = [];
        foreach (var req in requests)
        {
            try
            {
                User? newUser = await RegisterUser(req.Email, req.Name, User.StringToRole(req.Role));
                users.Add(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Request(Email:{req.Email}) exit with Error:{ex.Message}");
            }
        }
        return users;
    }
    private async Task<User?> FindByEmailOrDefault(string email)
    {
        var db = await _usersCollection;
        return await db.GetOneAsync(x => x.Email == email);
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
        int verifyCodeNumber = RandomNumberGenerator.GetInt32(0, 10000);
        var verfyCode = verifyCodeNumber.ToString("D4");

        _logger.LogInformation($"Reset Password Code:{verfyCode} generated for email: {email}");

        await StoreVerifyCode(user.Id, verfyCode);

        string templateDataString = JsonSerializer.Serialize(new { Name = user.Name, Code = verfyCode });
        var response = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            ToEmail = email,
            ToName = user.Name,
            TemplateName = "Welcome", 
            TemplateData = templateDataString
        });

        if (!response.Success)
        {
            _logger.LogError($"SendEmail failed: {response.Errors}");
            throw new InternalErrorException("Could not send reset email.");
        }
    }

    private async Task<User> UpdateUser(User userToUpdate)
    {
        var db = await _usersCollection;
        if (!await db.ExistsAsync(u => u.Id == userToUpdate.Id))
        {
            throw new NotFoundException($"User not found");
        }
        await db.ReplaceAsync(userToUpdate);
        return userToUpdate;
    }

    private async Task StoreVerifyCode(string userId, string code)
    {
        var db = await _verifyCollection;

        if (await db.ExistsAsync(v => v.UserId == userId))
        {
            var existing = await db.GetOneAsync(v => v.UserId == userId);
            if (existing != null) await db.DeleteWithIdAsync(existing.Id);
        }

        var verifyEntry = new VerifyCode
        {
            UserId = userId,
            Code = code,
            CreatedAt = DateTime.UtcNow
        };

        await db.InsertAsync(verifyEntry);
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

    private async Task<bool> HasVerifyCode(string userId)
    {
        var db = await _verifyCollection;
        return await db.ExistsAsync(v => v.UserId == userId);
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

    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, 13);
    private bool VerifyPassword(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);

    private string GenerateJwtToken(string userId, string userEmail, UserType role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["SecretKey"] ?? "a_very_secret_key_that_must_be_long_and_complex");
        var claims = new[]
        {
            new Claim(UserJwtExtensions.IdClaim, userId),
            new Claim(UserJwtExtensions.EmailClaim, userEmail),
            new Claim(UserJwtExtensions.RoleClaim, role.ToString().ToLowerInvariant()),
            new Claim(UserJwtExtensions.IsVerifiedClaim, "true")
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
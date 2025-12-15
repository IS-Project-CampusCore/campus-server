using BCrypt.Net;
using commons;
using emailServiceClient;
using excelServiceClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using users.Model;
using users.Utils;
using usersServiceClient;

namespace users;

public interface IUsersServiceImplementation 
{ 
    public Task<User?> GetUserById(string id);

    public Task<User?> RegisterUser(string email, string name, UserType role);

    public UserWithJwt AuthUser(string email, string password);

    public UserWithJwt Verify(string email, string password, string verifyCode);

    public Task<List<User?>> RegisterUsersFromExcel(string fileName);

}

public class UsersServiceImplementation(
    ILogger<UsersServiceImplementation> logger,
    IConfiguration config,
    emailService.emailServiceClient emailService,
    excelService.excelServiceClient excelService
    ) : IUsersServiceImplementation
{
    private readonly ILogger<UsersServiceImplementation> _logger = logger;
    private readonly IConfiguration _config = config;

    private readonly emailService.emailServiceClient _emailService = emailService;
    private readonly excelService.excelServiceClient _excelService = excelService;

    private List<User> _users = [];
    private readonly Dictionary<string, string> _verificationCodes = [];

    public Task<User?> GetUserById(string id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public UserWithJwt AuthUser(string email, string password)
    {
        User? user = FindByEmailOrDefault(email);

        if (user is null || !VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogError($"No user found for Email:{email}");
            throw new BadRequestException("Incorect Email or Password");
        }

        if (!user.IsVerified)
        {
            _logger.LogError($"User with Email:{email} is not verify");
            throw new BadRequestException($"The user:{user.Name} is not verified");
        }

        _logger.LogInformation($"User:{user} has been authenticated"); ;
        return new UserWithJwt(user, GenerateJwtToken(user.Id, user.Email, user.Role));
    }

    public async Task<User?> RegisterUser(string email, string name, UserType role)
    {
        User? user = FindByEmailOrDefault(email);
        if (user is not null)
        {
            _logger.LogError($"Email:{email} is already used");
            throw new BadRequestException($"Email:{email} is already used");
        }

        User newUser = new User
        {
            Id = RandomNumberGenerator.GetInt32(0, 10000).ToString(),
            Email = email,
            Name = name,
            Role = role,
            IsVerified = false,
        };

        int verifyCodeNumber = RandomNumberGenerator.GetInt32(0, 10000);
        var verfyCode = verifyCodeNumber.ToString("D4");

        _logger.LogInformation($"Verify Code:{verfyCode} generated for email: {email}");

        string templateDataString = JsonSerializer.Serialize(new
        {
            Name = name,
            Code = verfyCode
        });

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

        StoreVerifyCode(email, verfyCode);

        _users.Add(newUser);

        _logger.LogInformation($"User:{newUser} has been registered");
        return newUser;
    }

    public UserWithJwt Verify(string email, string password, string verifyCode)
    {
        User? user = FindByEmailOrDefault(email);
        if (user is null)
        {
            _logger.LogError($"No user found for Email:{email}");
            throw new BadRequestException("Incorect Email");
        }

        if (!HasVerifyCode(email) || IsVerify(email))
        {
            _logger.LogError($"User already verify");
            throw new BadRequestException("User already verify");
        }

        string? code = GetVerificationCode(email);
        if (code is null || code != verifyCode)
        {
            _logger.LogError($"Verify code did not match");
            throw new BadRequestException($"Verify code did not match");
        }

        try
        {
            RemoveVerificationCode(email);

            string passwordHash = HashPassword(password);

            User verifiedUser = new User
            {
                Id = user.Id,
                Email = email,
                Name = user.Name,
                PasswordHash = passwordHash,
                IsVerified = true,
                Role = user.Role,
            };

            UpdateUser(verifiedUser);

            return new UserWithJwt(verifiedUser, GenerateJwtToken(verifiedUser.Id, email, user.Role));
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

    public async Task<List<User?>> RegisterUsersFromExcel(string fileName)
    {
        if (fileName is null)
        {
            _logger.LogError("The file name is empty");
            throw new BadRequestException("No file selected");
        }

        var response = await _excelService.ParseExcelAsync(new ParseExcelRequest { FileName = fileName });

        if(!response.Success)
        {
            _logger.LogError($"ParseExcel failed for FileName:{fileName} whit Code:{response.Code} and Error:{response.Errors}");
            throw new InternalErrorException(response.Errors);
        }

        if(string.IsNullOrEmpty(response.Body))
        {
            _logger.LogError($"Excel FileName:{fileName} has no data");
            throw new BadRequestException("Empty excel file");
        }

        ExcelData data = response.GetPayload<ExcelData>()!;
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
                _logger.LogError($"Request(Email:{req.Email}, Name:{req.Name}, Role:{req.Role}) exit with Error:{ex.Message}");
            }
        }

        return users;
    }

    private User? FindByEmailOrDefault(string email) => _users.FirstOrDefault(x => x.Email == email);

    private User UpdateUser(User newUser)
    {
        User? oldUser = FindByEmailOrDefault(newUser.Email);
        if (oldUser is null)
        {
            _logger.LogError($"No user found for Email:{newUser.Email}");
            throw new BadRequestException($"There is no user registered with email:{newUser.Email}");
        }

        _users.Remove(oldUser);
        _users.Add(newUser);

        _logger.LogInformation($"User:{oldUser} has been updated to:{newUser}");
        return newUser;
    }

    private void StoreVerifyCode(string email, string code)
    {
        if(HasVerifyCode(email) && !IsVerify(email))
        {
            _logger.LogError($"There is a verify code associated to email:{email}");
            throw new BadRequestException($"There is a verify code associated to email:{email}");
        }

        _verificationCodes[email] = code;
    }

    private string? GetVerificationCode(string email) => _verificationCodes.TryGetValue(email, out var code) ? code : null;

    private void RemoveVerificationCode(string email)
    {
        if (!HasVerifyCode(email))
        {
            _logger.LogError($"There is no verification code associated to email:{email}");
            throw new BadRequestException($"There is no verification code associated to email:{email}");
        }

        _verificationCodes.Remove(email);
    }

    private bool HasVerifyCode(string email)
    {
        return _verificationCodes.ContainsKey(email);
    }

    private bool IsVerify(string email)
    {
        return FindByEmailOrDefault(email)?.IsVerified ?? false;
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
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

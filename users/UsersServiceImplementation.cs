using BCrypt.Net;
using commons;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using users.Model;

namespace users;

public interface IUsersServiceImplementation 
{ 
    public User? RegisterUser(string email, string name, UserType role);

    public UserWithJWT AuthUser(string email, string password);

    public UserWithJWT Verify(string email, string password, string verifyCode);

}

public class UsersServiceImplementation(ILogger<UsersServiceImplementation> logger, IConfiguration config) : IUsersServiceImplementation
{
    private readonly ILogger<UsersServiceImplementation> _logger = logger;
    private readonly IConfiguration _config = config;

    private List<User> _users = initializeMockData();
    private readonly Dictionary<string, string> _verificationCodes = [];

    public UserWithJWT AuthUser(string email, string password)
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
        return new UserWithJWT(user, GenerateJwtToken(user.Id, user.Email, user.Role));
    }

    public User? RegisterUser(string email, string name, UserType role)
    {
        User? user = FindByEmailOrDefault(email);
        if (user is not null)
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
        };

        string verfyCode = "1111";

        StoreVerifyCode(email, verfyCode);

        _users.Add(newUser);

        _logger.LogInformation($"User:{newUser} has been registered");
        return newUser;
    }

    public UserWithJWT Verify(string email, string password, string verifyCode)
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
                Email = email,
                Name = user.Name,
                PasswordHash = passwordHash,
                IsVerified = true,
                Role = user.Role,
            };

            UpdateUser(verifiedUser);

            return new UserWithJWT(verifiedUser, GenerateJwtToken(verifiedUser.Id, email, user.Role));
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

    public User? FindByEmailOrDefault(string email) => _users.FirstOrDefault(x => x.Email == email);

    public User UpdateUser(User newUser)
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

    public void StoreVerifyCode(string email, string code)
    {
        if(HasVerifyCode(email) && !IsVerify(email))
        {
            _logger.LogError($"There is a verify code associated to email:{email}");
            throw new BadRequestException($"There is a verify code associated to email:{email}");
        }

        _verificationCodes[email] = code;
    }

    public string? GetVerificationCode(string email) => _verificationCodes.TryGetValue(email, out var code) ? code : null;

    public void RemoveVerificationCode(string email)
    {
        if (!HasVerifyCode(email))
        {
            _logger.LogError($"There is no verification code associated to email:{email}");
            throw new BadRequestException($"There is no verification code associated to email:{email}");
        }

        _verificationCodes.Remove(email);
    }

    public bool HasVerifyCode(string email)
    {
        return _verificationCodes.ContainsKey(email);
    }

    public bool IsVerify(string email)
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
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Role, role.ToString())
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

    private static List<User> initializeMockData()
    {
        List<User> users = [];
        var student1 = new Student
        {
            Id = "123",
            Name = "Ion Popescu",
            Email = "birlea24@gmail.com",
            PasswordHash = "",
            IsVerified = false,
            University = "Universitatea tehnica din cluj napoce",
            Year = 2,
            Group = 1,
            Major = "Computer Science",
            Role = UserType.STUDENT
        };
        var professor1 = new Professor
        {
            Id = "2004",
            Name = "Maria Ionescu",
            Email = "maria.ionescu@campus.utcluj.ro",
            PasswordHash = "",
            IsVerified = false,
            University = "Universitatea tehnica din cluj napoca",
            Subjects = new List<string> { "Data Structures", "Algorithms" },
            Department = "Computer Science",
            Title = "Doctor",
            Role = UserType.PROFESSOR
        };

        users.Add(student1);
        users.Add(professor1);

        return users;
    }
}

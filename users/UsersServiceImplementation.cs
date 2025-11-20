using users.Model;

namespace users;

public interface IUsersServiceImplementation 
{ 
    public Task<User?> RegisterAsync(string email, string password);
    public Task<UserWithJWT> AuthAsync(string email, string password);
}

public class UsersServiceImplementation(ILogger<UsersServiceImplementation> logger) : IUsersServiceImplementation
{
    private readonly ILogger<UsersServiceImplementation> _logger = logger;

    public Task<UserWithJWT> AuthAsync(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<User?> RegisterAsync(string email, string password)
    {
        throw new NotImplementedException();
    }
}

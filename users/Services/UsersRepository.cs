namespace users.Services;

public class UsersRepository
{
    private readonly Dictionary<string, User> _users;
    private readonly Dictionary<string, string> _verificationCodes;

    public UsersRepository()
    {
        _users = new Dictionary<string, User>();
        _verificationCodes = new Dictionary<string, string>();

        initializeMockData();
    }

    public User? GetUserByEmail(string email)
    {
        return _users.TryGetValue(email, out var user) ? user : null;
    }

    public bool UserExists(string email)
    {
        return _users.ContainsKey(email);
    }

    public void UpdateUser(User user)
    {
        _users[user.email] = user;
    }

    public void AddUser(User user)
    {
        if (_users.ContainsKey(user.email))
        {
            throw new InvalidOperationException($"User with email {user.email} already exists");
        }
        _users[user.email] = user;
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _users.Values;
    }

    public void StoreVerificationCode(string email, string code)
    {
        _verificationCodes[email] = code;
    }
    public string? GetVerificationCode(string email)
    {
        return _verificationCodes.TryGetValue(email, out var code) ? code : null;
    }

    public void RemoveVerificationCode(string email)
    {
        _verificationCodes.Remove(email);
    }

    public bool HasVerificationCode(string email)
    {
        return _verificationCodes.ContainsKey(email);
    }

    private void initializeMockData()
    {
        var student1 = new Student(
            id: "123",
            name: "Ion Popescu",
            email: "ion.popescu@student.utcluj.ro",
            passwordHash: "",
            isVerified: false,
            university: "Universitatea tehnica din cluj napoce",
            year: 2,
            group: 1,
            major: "Computer Science"
            );
        var professor1 = new Professor(
            id: "2004",
            name: "Maria Ionescu",
            email: "maria.ionescu@campus.utcluj.ro",
            passwordHash: "",
            isVerified: false,
            university: "Universitatea tehnica din cluj napoca",
            subjects: new List<string> { "Data Structures", "Algorithms" },
            department: "Computer Science",
            title: "Doctor"
            );
        _users.Add(student1.email, student1);
        _users.Add(professor1.email, professor1);
    }
}


public record Admin(
    string id,
    string name,
    string email,
    string passwordHash,
    bool isVerified) : User(id,name,email, passwordHash, "Admin", isVerified);

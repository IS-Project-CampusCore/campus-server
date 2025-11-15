public abstract record User(
   string id,
    string name,
    string email,
    string passwordHash,
    string role,
    bool isVerified); 
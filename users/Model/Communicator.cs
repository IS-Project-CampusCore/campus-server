public abstract record Communicator(
    string id,
    string name,
    string email,
    string passwordHash,
    string role,
    bool isVerified) :User(id,name,email,passwordHash,role, isVerified);
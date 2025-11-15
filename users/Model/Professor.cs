public record Professor 
        (
        string id,
        string name,
        string email,
        string passwordHash,
        bool isVerified,
        string university,
        List<string> subjects,
        string department,
        string title
        ):Communicator(id,name,email, passwordHash, "Professor",isVerified);
    
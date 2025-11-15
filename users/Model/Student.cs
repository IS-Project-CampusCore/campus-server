  public record Student( 
        string id,
        string name,
        string email,
        string passwordHash,
        bool isVerified,
        string university,
        int year,
        int group,
        string major): Communicator(id,name, email, passwordHash,"Student",isVerified);
}
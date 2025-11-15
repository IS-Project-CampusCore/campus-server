public record Admin: User
{
    public Admin
        (
      
        string id,
        string name,
        string email,
        string passwordHash,
        bool isVerified
        ) 
        {
        Id=id;
        Name=name;
        Email=email; 
        PasswordHash=passwordHash;
        IsVerified=isVerified;
        Role = "Admin";
        }
}
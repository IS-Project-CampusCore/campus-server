public record Management: User //it think it should be names Manager but i just foloow the usecase
{
    public Management
    (
       string id,
       string name,
       string email,
       string passwordHash,
       bool isVerified
    )
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        IsVerified = isVerified;
        Role = "Management";
    }
}
public record Student: Comunicator
{
    public string University { get; set; }
    public int Year { get; set; }
    public int Group {  get; set; }
    public string Major {  get; set; }

    public Student() { }
    public Student( 
        string id,
        string name,
        string email,
        string passwordHash,
        bool isVerified,
        string university,
        int year,
        int group,
        string major)
    {
        Id = id;
        Name = name;
        Email = email;
        Role = "Student";
        PasswordHash = passwordHash;
        IsVerified = isVerified;
        Year = year;
        Group = group;
        Major = major;
           
    }
}
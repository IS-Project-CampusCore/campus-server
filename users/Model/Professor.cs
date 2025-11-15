public record Professor: Comunicator
{
    public string University { get; set; }
    public List<string>Subjects { get; set; }
    public string Department { get; set; }
    public string Title { get; set; }

    public Professor 
        (
        string id,
        string name,
        string email,
        string passwordhash,
        bool isVerified,
        string university,
        List<string> subjects,
        string department,
        string title
        )
    {
        Id = id;
        Name = name;
        Email = email;
        Role = "Professor";
        PasswordHash = passwordhash;
        IsVerified = isVerified;
        University = university;
        Subjects = subjects;
        Department = department;
        title = title;
    }

}
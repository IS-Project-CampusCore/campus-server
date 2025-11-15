public record CampusStudent: Student
{
    string campusNr;
    string roomNr;
    public CampusStudent
        (string id,
        string name,
        string email,
        string passwordHash,
        bool isVerified,
        string university,
        int year,
        int group,
        string major,
        string campusNr, 
        string roomNr
        )
        {
        Id= id; 
        Name = name;
        Email = email; 
        PasswordHash = passwordHash;
        IsVerified = isVerified;
        Year = year;
        Group = group;
        Major = major;
        this.campusNr = campusNr;
        this.roomNr = roomNr;
        }
}
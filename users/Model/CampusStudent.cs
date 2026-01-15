namespace users.Model;
public record CampusStudent() : Student(UserType.CAMPUS_STUDENT)
{
    public string Dormitory { get; set; } = string.Empty;
    public int Room { get; set; } = -1;
}

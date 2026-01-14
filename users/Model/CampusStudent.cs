namespace users.Model;
public record CampusStudent() : Student(UserType.CAMPUS_STUDENT)
{
    public required string Dormitory { get; set; } = string.Empty;
    public required int Room { get; set; } = -1;
}

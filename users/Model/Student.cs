using users.Model;

public record Student(UserType Role = UserType.STUDENT) : Communicator(Role)
{
    public required int Year { get; set; } = -1;
    public required int Group { get; set; } = -1;
    public required string Major {  get; set; } = string.Empty;
}
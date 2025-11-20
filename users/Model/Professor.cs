using users.Model;

public record Professor() : Communicator(UserType.PROFESSOR)
{
    public required List<string> Subjects { get; set; } = [];
    public required string Department { get; set; } = string.Empty;
    public string Title {  get; set; } = string.Empty;
}
    
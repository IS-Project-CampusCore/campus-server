namespace users.Model;

public record AllUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role {  get; set; } = string.Empty;
    public string University {  get; set; } = string.Empty;
    public int Year { get; set; } = -1;
    public int Group { get; set; } = -1;
    public string Major { get; set; } = string.Empty;
    public string Dormitory {  get; set; } = string.Empty;
    public int Room { get; set; } = -1;
    public string Department {  get; set; } = string.Empty;
    public string Title {  get; set; } = string.Empty;
}

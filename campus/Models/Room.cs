using commons.Database;

namespace campus.Models;

[CollectionName("Rooms")]
public record Room : DatabaseModel
{
    public string DormitoryId { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 0;
    public List<string> MembersId { get; set; } = [];
}
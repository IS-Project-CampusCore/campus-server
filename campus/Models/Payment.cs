using commons.Database;

namespace campus.Models;

[CollectionName("Payments")]
public record Payment : DatabaseModel
{
    public string UserId { get; set; }= string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastPaymentDate { get; set; }
    public DateTime NextPaymentDate { get; set; }
}
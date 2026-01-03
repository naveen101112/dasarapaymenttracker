namespace dasarapaymenttracker.Models;

public class PaymentStatus
{
    public int PaymentStatusId { get; set; }
    public int PeerId { get; set; }
    public int PersonId { get; set; }
    public int PayMonthId { get; set; }

    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

namespace dasarapaymenttracker.Models;

public class Person
{
    public int PersonId { get; set; }
    public int PeerId { get; set; }
    public string PersonName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Peer? Peer { get; set; }
    public List<PaymentStatus> PaymentStatuses { get; set; } = new();
}

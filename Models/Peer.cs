namespace dasarapaymenttracker.Models;

public class Peer
{
    public int PeerId { get; set; }
    public string PeerName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public List<Person> People { get; set; } = new();
}

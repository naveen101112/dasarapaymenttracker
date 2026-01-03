using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using dasarapaymenttracker.Data;
using dasarapaymenttracker.Models;

namespace dasarapaymenttracker.Pages;

public class PeersModel : PageModel
{
    private readonly AppDbContext _db;
    public PeersModel(AppDbContext db) => _db = db;
    public List<Peer> Peers { get; set; } = new();

    public async Task OnGetAsync()
        => Peers = await _db.Peers.Where(x => x.IsActive).ToListAsync();
}

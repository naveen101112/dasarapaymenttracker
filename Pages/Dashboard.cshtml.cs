using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using dasarapaymenttracker.Data;
using dasarapaymenttracker.Models;
using dasarapaymenttracker.Services;

namespace dasarapaymenttracker.Pages;
public class DashboardModel : PageModel
{
    private readonly AppDbContext _db;

    public DashboardModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int PeerId { get; set; }

    public string PeerName { get; set; } = "";
    public string? LockedMonthLabel { get; set; }

    public string Message { get; set; } = "";
    public string Error { get; set; } = "";

    public List<MonthVm> Months { get; set; } = new();
    public List<RowVm> Rows { get; set; } = new();

    public class MonthVm
    {
        public int PayMonthId { get; set; }
        public DateOnly MonthStartDate { get; set; }
        public string Label { get; set; } = "";
        public bool IsLocked { get; set; }
    }

    public class CellVm
    {
        public int PayMonthId { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLocked { get; set; }
    }

    public class RowVm
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; } = "";
        public List<CellVm> Cells { get; set; } = new();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (PeerId <= 0) return RedirectToPage("/Peers");

        var peer = await _db.Peers.FirstOrDefaultAsync(p => p.PeerId == PeerId && p.IsActive);
        if (peer == null) return RedirectToPage("/Peers");

        PeerName = peer.PeerName;

        await LoadGridAsync();
        return Page();
    }

    // Handler name used in Dashboard.cshtml: asp-page-handler="AddPerson"
    public async Task<IActionResult> OnPostAddPersonAsync(int peerId, string personName)
    {
        PeerId = peerId;

        if (string.IsNullOrWhiteSpace(personName))
            return RedirectToPage(new { peerId = PeerId });

        try
        {
            _db.People.Add(new Person
            {
                PeerId = peerId,
                PersonName = personName.Trim()
            });

            await _db.SaveChangesAsync();
            Message = "Name added.";
        }
        catch
        {
            Error = "Could not add name (maybe duplicate under same peer).";
        }

        return RedirectToPage(new { peerId = PeerId });
    }

    // Handler name used in Dashboard.cshtml: asp-page-handler="TogglePayment"
    public async Task<IActionResult> OnPostTogglePaymentAsync(int peerId, int personId, int payMonthId, string newValue)
    {
        PeerId = peerId;

        var person = await _db.People.FirstOrDefaultAsync(x => x.PersonId == personId && x.PeerId == peerId && x.IsActive);
        if (person == null)
        {
            Error = "Invalid person/peer.";
            await LoadGridAsync();
            return Page();
        }

        var month = await _db.PayMonths.FirstOrDefaultAsync(m => m.PayMonthId == payMonthId);
        if (month == null)
        {
            Error = "Invalid month.";
            await LoadGridAsync();
            return Page();
        }

        // Server-side lock enforcement
        var ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);

        if (MonthLockService.IsLocked(nowLocal, month.MonthStartDate))
        {
            Error = "This month is locked (after 15th rule).";
            await LoadGridAsync();
            return Page();
        }


        var status = await _db.PaymentStatuses
            .FirstOrDefaultAsync(x => x.PersonId == personId && x.PayMonthId == payMonthId);

        if (status == null)
        {
            _db.PaymentStatuses.Add(new PaymentStatus
            {
                PeerId = peerId,
                PersonId = personId,
                PayMonthId = payMonthId,
                IsPaid = !string.IsNullOrEmpty(newValue),
                PaidAt = !string.IsNullOrEmpty(newValue) ? DateTime.UtcNow : null,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            status.IsPaid = !string.IsNullOrEmpty(newValue);
            status.PaidAt = !string.IsNullOrEmpty(newValue) ? DateTime.UtcNow : null;
            status.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return RedirectToPage(new { peerId = PeerId });
    }

    private async Task LoadGridAsync()
    {
        // Peer Name
        var peer = await _db.Peers.FirstAsync(p => p.PeerId == PeerId);
        PeerName = peer.PeerName;

        // Months (Oct 2025 - Sep 2026) from DB (seeded)
        var monthsDb = await _db.PayMonths
            .OrderBy(m => m.MonthStartDate)
            .ToListAsync();

        var ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);

        var (prevLocked, currLocked) = MonthLockService.LockedMonths(nowLocal);

        Months = monthsDb.Select(m => new MonthVm
        {
            PayMonthId = m.PayMonthId,
            MonthStartDate = m.MonthStartDate,
            Label = m.MonthStartDate.ToString("MMM yyyy"),
            IsLocked =
                (prevLocked.HasValue && prevLocked.Value == m.MonthStartDate) ||
                (currLocked.HasValue && currLocked.Value == m.MonthStartDate),
            
        }).ToList();

        LockedMonthLabel = (prevLocked, currLocked) switch
        {
            (null, null) => null,
            (var p, var c) when p.HasValue && c.HasValue => $"{p:MMM yyyy} & {c:MMM yyyy}",
            (var p, null) when p.HasValue => $"{p:MMM yyyy}",
            (null, var c) when c.HasValue => $"{c:MMM yyyy}",
            _ => null
        };



        // People under peer
        var people = await _db.People
            .Where(p => p.PeerId == PeerId && p.IsActive)
            .OrderBy(p => p.PersonName)
            .ToListAsync();

        // Payment map for this peer
        var statuses = await _db.PaymentStatuses
            .Where(s => s.PeerId == PeerId)
            .ToListAsync();

        var map = statuses.ToDictionary(k => (k.PersonId, k.PayMonthId), v => v.IsPaid);

        Rows = new List<RowVm>();
        foreach (var person in people)
        {
            var row = new RowVm
            {
                PersonId = person.PersonId,
                PersonName = person.PersonName
            };

            foreach (var m in Months)
            {
                map.TryGetValue((person.PersonId, m.PayMonthId), out var paid);

                row.Cells.Add(new CellVm
                {
                    PayMonthId = m.PayMonthId,
                    IsPaid = paid,
                    IsLocked = m.IsLocked
                });
            }

            Rows.Add(row);
        }
    }

}

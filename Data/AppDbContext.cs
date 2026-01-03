using Microsoft.EntityFrameworkCore;
using dasarapaymenttracker.Models;

namespace dasarapaymenttracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Peer> Peers => Set<Peer>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PayMonth> PayMonths => Set<PayMonth>();
    public DbSet<PaymentStatus> PaymentStatuses => Set<PaymentStatus>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Peer>().HasIndex(x => x.PeerName).IsUnique();
        b.Entity<Person>().HasIndex(x => new { x.PeerId, x.PersonName }).IsUnique();
        b.Entity<PayMonth>().HasIndex(x => x.MonthStartDate).IsUnique();
        b.Entity<PaymentStatus>().HasIndex(x => new { x.PersonId, x.PayMonthId }).IsUnique();
    }
}

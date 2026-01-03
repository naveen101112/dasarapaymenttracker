using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using dasarapaymenttracker.Data;
using dasarapaymenttracker.Models;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/Login");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Login";
        o.Cookie.Name = "PaymentTrackerAuth";
    });

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// DB migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.PayMonths.Any())
    {
        var d = new DateOnly(2025, 10, 1);
        for (int i = 0; i < 12; i++)
        {
            db.PayMonths.Add(new PayMonth { MonthStartDate = d.AddMonths(i) });
        }
        db.SaveChanges();
    }

    if (!db.Peers.Any())
    {
        db.Peers.AddRange(
            new Peer { PeerName = "Peer-A" },
            new Peer { PeerName = "Peer-B" }
        );
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/Logout", async ctx =>
{
    await ctx.SignOutAsync();
    ctx.Response.Redirect("/Login");
});

app.MapRazorPages();
app.Run();
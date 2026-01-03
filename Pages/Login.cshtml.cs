using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace dasarapaymenttracker.Pages;

public class LoginModel : PageModel
{
    private readonly IConfiguration _cfg;
    public LoginModel(IConfiguration cfg) => _cfg = cfg;

    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string Error { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (Username == _cfg["Auth:Username"] &&
            Password == _cfg["Auth:Password"])
        {
            var id = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, Username) },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));

            return RedirectToPage("/Peers");
        }

        Error = "Invalid login";
        return Page();
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Flash.SensitiveWords.Web.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("account/login")]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("account/login")]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        // Validate empty fields
        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError(nameof(username), "Username is required");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(nameof(password), "Password is required");
        }

        // If there are validation errors, return the view
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var adminUser = _configuration["Admin:Username"];
        var adminPass = _configuration["Admin:Password"];

        if (!string.IsNullOrEmpty(adminUser) && username == adminUser && password == adminPass)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "SensitiveWords");
        }

        ModelState.AddModelError(string.Empty, "Invalid username or password");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("account/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Chat");
    }
}

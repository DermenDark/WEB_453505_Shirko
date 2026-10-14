using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LRs.Controllers;

[AllowAnonymous]
public sealed class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var redirectUri = !string.IsNullOrWhiteSpace(returnUrl) &&
                          Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Content("~/");

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = redirectUri
            },
            "keycloak");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties
            {
                RedirectUri = Url.Content("~/")
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            "keycloak");
    }
}

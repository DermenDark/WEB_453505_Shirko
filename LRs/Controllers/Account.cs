
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LRs.Controllers
{
    public class Account : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task Login(string? returnUrl = null)
        {
            var redirectUri =
                !string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl)
                    ? returnUrl
                    : Url.Action("Index", "Home") ?? "/";

            await HttpContext.ChallengeAsync(
                "keycloak",
                new AuthenticationProperties
                {
                    RedirectUri = redirectUri
                });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignOutAsync(
                "keycloak",
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home") ?? "/"
                });
        }
    }
}

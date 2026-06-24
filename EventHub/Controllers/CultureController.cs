using EventHub.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers;

public class CultureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetCulture(string culture, string? returnUrl)
    {
        var selectedCulture = SupportedCultures.IsSupported(culture)
            ? culture.Trim().ToLowerInvariant()
            : SupportedCultures.DefaultCulture;

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selectedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/"));
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace OrganicOption.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class LanguageController : Controller
    {
        private readonly ILogger<LanguageController> _logger;

        public LanguageController(ILogger<LanguageController> logger)
        {
            _logger = logger;
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
    {
        try
        {
          
            Console.WriteLine($"Incoming returnUrl: {returnUrl}");

            // Check if the returnUrl is local
            if (!Url.IsLocalUrl(returnUrl))
            {
                // Log if the returnUrl is not local
                Console.WriteLine($"ReturnUrl is not local. Redirecting to Home/Index.");

                // Set returnUrl to a default local URL
                returnUrl = Url.Action("Index", "Home");
            }
            
            // Log the final returnUrl before redirecting
            Console.WriteLine($"Final returnUrl: {returnUrl}");

            // Set the culture cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

          
            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
           
            Console.WriteLine($"Exception occurred in SetLanguage: {ex.Message}");
            throw;
        }
    }
    }
}

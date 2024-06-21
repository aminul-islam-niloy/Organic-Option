using Microsoft.AspNetCore.Mvc;

namespace OrganicOption.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

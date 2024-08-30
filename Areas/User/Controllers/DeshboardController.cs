using Microsoft.AspNetCore.Mvc;

namespace OrganicOption.Areas.User.Controllers
{
    [Area("User")]
    public class DeshboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}

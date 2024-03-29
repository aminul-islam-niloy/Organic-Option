using Microsoft.AspNetCore.Mvc;

namespace OrganicOption.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

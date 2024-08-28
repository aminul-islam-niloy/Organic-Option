using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OrganicOption.Models.Blogs;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicOption.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,Name,Email")] FeedBack feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                else
                {
                    return RedirectToAction(nameof(ThankYou));
                }
            }

            return View(feedback);
        }


        public IActionResult ThankYou()
        {
            return PartialView(); 
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View( _context.FeedBack.ToList());
        }
    }
}

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

        // GET: Feedback/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,Name,Email")] FeedBack feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ThankYou));
            }
            return View(feedback);
        }

        // GET: Feedback/ThankYou
        public IActionResult ThankYou()
        {
            return View();
        }

   
        public IActionResult Index()
        {
            return View( _context.FeedBack.ToList());
        }
    }
}

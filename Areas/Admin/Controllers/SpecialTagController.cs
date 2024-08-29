using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using System.Linq;
using System.Threading.Tasks;


namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SpecialTagController : Controller
    {
        private ApplicationDbContext _db;

        public SpecialTagController(ApplicationDbContext db)
        {
            _db = db;   
        }


        public IActionResult Index()
        {
            var data = _db.SpecialTag.ToList();
            return View(data);
           
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.SpecialTag specialTag)
        {
            if (ModelState.IsValid)
            {
                _db.SpecialTag.Add(specialTag);
                await _db.SaveChangesAsync();
                TempData["create"] = "Tag type has been Added";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }


        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            var specialTag = _db.SpecialTag.Find(id);
            if (specialTag == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            return View(specialTag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.SpecialTag specialTag)
        {
            if (ModelState.IsValid)
            {
                _db.Update(specialTag);
                await _db.SaveChangesAsync();
                TempData["edit"] = "Tag type has been Updated";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }

        //GET Details Action Method

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var specialTag = _db.SpecialTag.Find(id);
            if (specialTag == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            return View(specialTag);
        }

        //POST Edit Action Method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(Models.SpecialTag specialTag)
        {
            return RedirectToAction(nameof(Index));

        }



        //GET Delete Action Method

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var specialTag = _db.SpecialTag.Find(id);
            if (specialTag == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            return View(specialTag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id, Models.SpecialTag specialTag)
        {
            if (id == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            if (id != specialTag.Id)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var specialTags = _db.SpecialTag.Find(id);
            if (specialTags == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            if (ModelState.IsValid)
            {
                _db.Remove(specialTags);
                await _db.SaveChangesAsync();
                TempData["delete"] = "Tag type has been deleted";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }



    }
}

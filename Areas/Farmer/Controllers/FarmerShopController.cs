using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OrganicOption.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicOption.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    [Authorize]
    public class FarmerShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        UserManager<IdentityUser> _userManager;
        public FarmerShopController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }


        //public async Task< IActionResult >Index()
        //{
        //    var farmerShop = await _context.FarmerShop
        //        .Include(fs => fs.Products) // Eager loading to include products
        //        .ToListAsync();

        //    return View(farmerShop);


        //}


        public async Task<IActionResult> Index()
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve the shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return RedirectToAction(nameof(CreateShop)); // Current user does not have a shop
            }

            return View(farmerShop);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FarmerShop farmerShop, IFormFile coverPhoto)
        {
            //if (ModelState.IsValid)
            //{
          
            // Handle the cover photo upload
            if (coverPhoto != null && coverPhoto.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await coverPhoto.CopyToAsync(stream);
                        farmerShop.CoverPhoto = stream.ToArray();
                    }
                }
                // Set the current user as the farmer for this shop
                var currentUser = await _userManager.GetUserAsync(User);

            var existingShop = await _context.FarmerShop.FirstOrDefaultAsync(shop => shop.FarmerUserId == currentUser.Id);
            if (existingShop != null)
            {
                // Redirect the user or display a message indicating they cannot create another shop
                return RedirectToAction(nameof(ShopExist)); // Example: Redirect to an error page indicating that the user already has a shop
            }

            farmerShop.FarmerUser = (OnlineShop.Models.ApplicationUser)currentUser;
                //farmerShop.FarmerUserId = User.Identity.Name;

                // Add the farmer shop to the database
                _context.FarmerShop.Add(farmerShop);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            //}
            //return View(farmerShop);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerShop = _context.FarmerShop.Find(id);
            if (farmerShop == null)
            {
                return NotFound();
            }

            return View(farmerShop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FarmerShop farmerShop, IFormFile coverPhoto)
        {
            if (id != farmerShop.Id)
            {
                return NotFound();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            farmerShop.FarmerUser = (OnlineShop.Models.ApplicationUser)currentUser;

            try
                {
                if (coverPhoto != null && coverPhoto.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await coverPhoto.CopyToAsync(stream);
                        farmerShop.CoverPhoto = stream.ToArray();
                    }
                }

                _context.Update(farmerShop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FarmerShopExists(farmerShop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
          
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerShop = await _context.FarmerShop
                .FirstOrDefaultAsync(m => m.Id == id);
            if (farmerShop == null)
            {
                return NotFound();
            }

            return View(farmerShop);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var farmerShop = await _context.FarmerShop.FindAsync(id);
            _context.FarmerShop.Remove(farmerShop);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FarmerShopExists(int id)
        {
            return _context.FarmerShop.Any(e => e.Id == id);
        }

        public IActionResult ShopExist()
        {
            return View();
        }

        public IActionResult CreateShop()
        {
            return View();
        }





    }
}

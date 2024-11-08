using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnlineShop.Data;
using OrganicOption.Models;
using System.Linq;
using System.Security.Claims;

namespace OrganicOption.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class FavoritesController : Controller
    {

        private readonly ILogger<FavoritesController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public FavoritesController(ILogger<FavoritesController> 
            logger, ApplicationDbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

   
        [HttpPost]
        public IActionResult AddToFavorites(int shopId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingFavorite = _context.Favorite.FirstOrDefault(f => f.FarmerShopId == shopId && f.UserId == userId);

            if (existingFavorite != null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var favorite = new Favorite { FarmerShopId = shopId, UserId = userId };

            _context.Favorite.Add(favorite);
            _context.SaveChanges();

            return Ok(); 
        }


        public IActionResult FavoriteShop()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteShop = _context.Favorite
                .Where(f => f.UserId == userId)
                .Select(f => f.FarmerShop)
                .ToList();

            return View(favoriteShop);
        }


        [HttpPost]
        public IActionResult RemoveFromFavorites(int shopId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteToRemove = _context.Favorite.FirstOrDefault(f => f.UserId == userId && f.FarmerShopId == shopId);
            if (favoriteToRemove != null)
            {
                _context.Favorite.Remove(favoriteToRemove);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }

    }
}

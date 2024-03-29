using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineShop.Data;
using OrganicOption.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicOption.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    public class InventoryController : Controller
    {

        private readonly ApplicationDbContext _context;
        UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;
        public InventoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }



        //Inventory

        // Action method to show all products in the shop
        public async Task<IActionResult> ShowAllProducts()
        {
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");
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


            return View(farmerShop);

        }

        // Action method to show sold products
        public async Task<IActionResult> ShowSoldProducts()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }
            // Retrieve the shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            var ordersToday = await _context.OrderDetails
                .Include(od => od.Order).ThenInclude(o => o.User).Where(od => od.Product.FarmerShopId == farmerShop.Id && od.Order.OrderDate.Date == DateTime.Today).ToListAsync();

            // Extract information about each sold product along with customer details
            var soldProductsToday = ordersToday
                .Select(od => new
                {
                    ProductId = od.Product.Id,
                    ProductName = od.Product.Name,
                    QuantitySold = od.Quantity,
                    TotalPrice = od.Quantity * od.Product.Price,
                    CustomerName = od.Order.Name,
                    CustomerPhone = od.Order.PhoneNo,
                    CustomerAddress = od.Order.Address
                })
                .Distinct()
                .ToList();

            ViewBag.SoldProductsToday = soldProductsToday;


            return View();
        }

        // Action method to calculate and show total revenue
        public IActionResult ShowTotalRevenue(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            var totalRevenue = farmerShop.CalculateTotalRevenue();
            return View(totalRevenue);
        }

        // Action method to find and show the most sold product
        public IActionResult ShowMostSoldProduct(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            var mostSoldProduct = farmerShop.FindMostSoldProduct();
            return View(mostSoldProduct);
        }

        // Action method to show daily sales
        public IActionResult ShowDailySales(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            int dailySales = farmerShop.GetDailySales();
            return View(dailySales);
        }


        public IActionResult CashoutRevenue(int farmerShopId, CashoutInterval interval)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            // Calculate revenue for the specified interval
            decimal revenue = farmerShop.CalculateAndCashoutRevenue(interval);

            // Award bonuses for best-selling shops
            farmerShop.AwardBonusesForBestSellingShops();

            // Return the remaining revenue after platform fee deduction
            return View(revenue);
        }






    }
}

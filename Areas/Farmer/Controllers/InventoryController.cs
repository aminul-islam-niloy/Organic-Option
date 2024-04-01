using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using OnlineShop.Data;
using OnlineShop.Models;
using OrganicOption.Models;
using OrganicOption.Models.Farmer_Section;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ProductInfo = OrganicOption.Models.Farmer_Section.ProductInfo;

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


        public async Task<IActionResult> MyDailyOrders()
        {
            // Retrieve the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return NotFound(); // Handle if farmer shop not found
            }

  

            var orders = await _context.Orders
                       .Include(o => o.OrderDetails.Where(od => od.Product.FarmerShopId == farmerShop.Id))
                       .ThenInclude(od => od.Product)
                       .Where(o => o.OrderDate.Date == DateTime.Today && o.InventoryItems.Any(ii => ii.FarmerShopId == farmerShop.Id))
                         .ToListAsync();


            var dailyOrderInfo = orders.Select(o => new DailyOrderInfoViewModel
            {
                OrderId = o.Id,
                CustomerName = o.Name,
                Address = o.Address,
                Phone = o.PhoneNo,
                OrderTme= o.OrderDate,
                Products = o.OrderDetails.Select(od => new ProductInfo
                {
                    Name = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Product.Price
                }).ToList(),
                TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Product.Price)
            }).ToList();

            return View(dailyOrderInfo);
        }



        //public async Task<IActionResult> WeeklyOrders()
        //{
        //    // Retrieve the current user
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    if (currentUser == null)
        //    {
        //        return NotFound();
        //    }

        //    // Retrieve the farmer shop for the current user
        //    var farmerShop = await _context.FarmerShop
        //        .Include(fs => fs.Products)
        //        .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

        //    if (farmerShop == null)
        //    {
        //        return NotFound(); // Handle if farmer shop not found
        //    }

        //    DateTime startDate = DateTime.Today.AddDays(-7);
        //    DateTime endDate = DateTime.Today;

        //    var ordersThisWeek = await _context.Orders
        //        .Include(o => o.OrderDetails.Where(od => od.Product.FarmerShopId == farmerShop.Id))
        //            .ThenInclude(od => od.Product)
        //        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.InventoryItems.Any(ii => ii.FarmerShopId == farmerShop.Id))
        //        .ToListAsync();

        //    var dailyOrderInfo = ordersThisWeek.Select(o => new DailyOrderInfoViewModel
        //    {
        //        OrderId = o.Id,
        //        CustomerName = o.Name,
        //        Address = o.Address,
        //        Phone = o.PhoneNo,
        //        OrderTme = o.OrderDate,
        //        Products = o.OrderDetails.Select(od => new ProductInfo
        //        {
        //            Name = od.Product.Name,
        //            Quantity = od.Quantity,
        //            Price = od.Product.Price
        //        }).ToList(),
        //        TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Product.Price)
        //    }).ToList();

        //    return View(dailyOrderInfo);
        //}


        public async Task<IActionResult> WeeklyOrders()
        {
            // Retrieve the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return NotFound(); // Handle if farmer shop not found
            }

            DateTime startDate = DateTime.Today.AddDays(-7);
            DateTime endDate = DateTime.Today;

            var ordersThisWeek = await _context.Orders
                .Include(o => o.OrderDetails.Where(od => od.Product.FarmerShopId == farmerShop.Id))
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.InventoryItems.Any(ii => ii.FarmerShopId == farmerShop.Id))
                .ToListAsync();

            var dailyOrderInfo = ordersThisWeek.Select(o => new DailyOrderInfoViewModel
            {
                OrderId = o.Id,
                CustomerName = o.Name,
                Address = o.Address,
                Phone = o.PhoneNo,
                OrderTme = o.OrderDate,
                Products = o.OrderDetails.Select(od => new ProductInfo
                {
                    Name = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Product.Price
                }).ToList(),
                TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Product.Price)
            }).ToList();

            var groupedOrders = dailyOrderInfo.GroupBy(o => o.OrderTme.Date).ToList();

            return View(groupedOrders);
        }




        public async Task<IActionResult> MonthlyOrders()
        {
            // Retrieve the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return NotFound(); // Handle if farmer shop not found
            }

            DateTime startDate = DateTime.Today.AddDays(-30);
            DateTime endDate = DateTime.Today;

            var ordersThisWeek = await _context.Orders
                .Include(o => o.OrderDetails.Where(od => od.Product.FarmerShopId == farmerShop.Id))
                    .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.InventoryItems.Any(ii => ii.FarmerShopId == farmerShop.Id))
                .ToListAsync();

            var dailyOrderInfo = ordersThisWeek.Select(o => new DailyOrderInfoViewModel
            {
                OrderId = o.Id,
                CustomerName = o.Name,
                Address = o.Address,
                Phone = o.PhoneNo,
                OrderTme = o.OrderDate,
                Products = o.OrderDetails.Select(od => new ProductInfo
                {
                    Name = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Product.Price
                }).ToList(),
                TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Product.Price)
            }).ToList();

            return View(dailyOrderInfo);
        }

        public async Task<IActionResult> AllOrdersGroupedByDate()
        {
            // Retrieve the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return NotFound(); // Handle if farmer shop not found
            }

            // Define the date range for past days and the present day
            DateTime startDate = DateTime.Today.AddDays(-30); // Change the number of days as needed
            DateTime endDate = DateTime.Today.AddDays(1).AddTicks(-1);

            // Retrieve orders for the specified date range
            var allOrders = await _context.Orders
                .Include(o => o.OrderDetails.Where(od => od.Product.FarmerShopId == farmerShop.Id))
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.InventoryItems.Any(ii => ii.FarmerShopId == farmerShop.Id))
                .ToListAsync();

            // Convert orders to DailyOrderInfoViewModel
            var dailyOrderInfo = allOrders.Select(o => new DailyOrderInfoViewModel
            {
                OrderId = o.Id,
                CustomerName = o.Name,
                Address = o.Address,
                Phone = o.PhoneNo,
                OrderTme = o.OrderDate,
                Products = o.OrderDetails.Select(od => new ProductInfo
                {
                    Name = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Product.Price
                }).ToList(),
                TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Product.Price)
            }).ToList();

            // Group dailyOrderInfo by date
            var groupedOrders = dailyOrderInfo.GroupBy(o => o.OrderTme.Date).ToList();

            return View(groupedOrders);
        }






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

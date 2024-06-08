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
using OrganicOption.Service;
using Stripe;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ProductInfo = OrganicOption.Models.Farmer_Section.ProductInfo;

namespace OrganicOption.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    [Authorize(Roles = "Farmer")]
    public class InventoryController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;
        public InventoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cache = memoryCache;
            _notificationService = notificationService;
        }


        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var currentShop = await _context.FarmerShop.FirstOrDefaultAsync(shop => shop.FarmerUserId == currentUser.Id);
       
               int farmerShopId = currentShop.Id;


            var salesInfo = _context.InventoryItem
       .Where(item => item.FarmerShopId == farmerShopId && item.OrderId != null)
       .GroupBy(item => item.ProductId)
       .Select(group => new
       {
           ProductId = group.Key,
           TotalItemsSold = group.Count(), // Count each ID even if it appears multiple times
           TotalQuantitySold = group.Sum(item => item.Quantity),
           TotalPriceSold = group.Sum(item => item.Price * item.Quantity)
       })
       .ToList();

            var totalSumPriceSold = salesInfo.Sum(item => item.TotalPriceSold);

            ViewBag.SalesInfo = salesInfo;
            ViewBag.TotalSumPriceSold = totalSumPriceSold;


            // Retrieve the shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop != null)
            {
                // Find the unsold products for the farmer
                var TotalsoldProducts = farmerShop.Products
                    .Where(p => _context.InventoryItem.Any(item => item.ProductId == p.Id))
                    .ToList();
                ViewBag.TotalSoldProduct = TotalsoldProducts;
                // Do something with unsoldProducts
            }


            if (farmerShop != null)
            {
                // Find the unsold products for the farmer
                var unsoldProducts = farmerShop.Products
                    .Where(p => !_context.InventoryItem.Any(item => item.ProductId == p.Id))
                    .ToList();
                ViewBag.UnsoldProduct = unsoldProducts;
                // Do something with unsoldProducts
            }






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

        public async Task<IActionResult> ShowAllProductsByTime()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return NotFound();
            }

            // Sort the products based on expiration time
            var sortedProducts = farmerShop.Products.OrderBy(p => p.ExpirationTime);

            return View(sortedProducts);
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

        //public async Task<IActionResult> AllOrdersGroupedByDate()
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

        //    // Define the date range for past days and the present day
        //    DateTime startDate = DateTime.Today.AddDays(-30); // Change the number of days as needed
        //    DateTime endDate = DateTime.Today.AddDays(1).AddTicks(-1);

        //    // Retrieve orders for the specified date range
        //    var allOrders = await _context.Orders
        //        .Include(o => o.OrderDetails.Where(od => od.Product.FarmerShopId == farmerShop.Id))
        //        .ThenInclude(od => od.Product)
        //        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.InventoryItems.Any(ii => ii.FarmerShopId == farmerShop.Id))
        //        .ToListAsync();

        //    // Convert orders to DailyOrderInfoViewModel
        //    var dailyOrderInfo = allOrders.Select(o => new DailyOrderInfoViewModel
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

        //    // Group dailyOrderInfo by date
        //    var groupedOrders = dailyOrderInfo.GroupBy(o => o.OrderTme.Date).ToList();

        //    return View(groupedOrders);
        //}



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
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.OrderDetails.Any(od => od.Product.FarmerShopId == farmerShop.Id))
                .ToListAsync();

            // Retrieve deliveries for the specified date range
            var allDeliveries = await _context.Deliveries
                .Where(d => allOrders.Select(o => o.Id).Contains(d.OrderId))
                .ToListAsync();

            // Convert orders to DailyOrderInfoViewModel and map deliveries
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
                TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Product.Price),
                OrderCondition = allDeliveries.FirstOrDefault(d => d.OrderId == o.Id)?.OrderCondition ?? OrderCondition.Onlist // Get the OrderCondition from Delivery
            }).ToList();

            // Group dailyOrderInfo by date
            var groupedOrders = dailyOrderInfo.GroupBy(o => o.OrderTme.Date).ToList();

            return View(groupedOrders);
        }



        [Authorize(Roles = "Farmer")]
        [HttpPost]
        public async Task<IActionResult> ReleaseOrder(int orderId)
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

            // Get the order and its details
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Update the order condition in the Order table
            order.OrderCondition = OrderCondition.OnDelivary;

            // Get the delivery associated with this order
            var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);

            if (delivery != null)
            {
                // Update the order condition in the Delivery table
                delivery.OrderCondition = OrderCondition.OnDelivary;
            }

            // Notify the customer
            var customerUserId = order.UserId; // Assuming UserId is the customer's Id
            _notificationService.AddNotification(customerUserId, $"Your order #{order.Id} is on the way. You can track it.", order.Id);

            // Update the farmer's total revenue for the specific shop
            foreach (var orderDetail in order.OrderDetails)
            {
                var product = orderDetail.Product;
                if (product.FarmerShopId == farmerShop.Id)
                {
                    farmerShop.ShopRevenue += orderDetail.Price * orderDetail.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            // Optionally, return to a relevant view or action
            return RedirectToAction("AllOrdersGroupedByDate");
        }



        // Action method to find and show the most sold product
        public IActionResult ShowMostSoldProduct()
        {
            return View();
        }

        // Action method to show daily sales
    


        public IActionResult CashoutRevenue()
        {
            return View();
        }






    }
}

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
using OrganicOption.Models.Rider_Section;
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
              
            }


            if (farmerShop != null)
            {
                // Find the unsold products for the farmer
                var unsoldProducts = farmerShop.Products
                    .Where(p => !_context.InventoryItem.Any(item => item.ProductId == p.Id))
                    .ToList();
                ViewBag.UnsoldProduct = unsoldProducts;
               
            }






            return View();
        }



        //Inventory

        //  all products in the shop
        public async Task<IActionResult> ShowAllProducts()
        {
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
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
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
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
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
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
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" }); // Handle if farmer shop not found
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
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" }); 
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
         
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
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
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" }); 
            }

            // past 90 days and the present day
            DateTime startDate = DateTime.Today.AddDays(-90); 
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


        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {

                return RedirectToAction(nameof(AllOrdersGroupedByDate));
            }

            var farmerShop = await _context.FarmerShop
                .Include(s => s.ShopAddress)
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {

                return RedirectToAction(nameof(AllOrdersGroupedByDate));
            }

            var order = await _context.Orders
                .Include(o => o.CustomerAddress)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.FarmerShop)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
               
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" }); 
            }

            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            if(delivery == null)
            {
                return RedirectToAction(nameof(AllOrdersGroupedByDate));
            }

            RiderModel rider = null;
            if (delivery != null)
            {
                rider = await _context.RiderModel
                    .Include(s => s.RiderAddress)
                    .FirstOrDefaultAsync(r => r.Id == delivery.RiderId);
            }

            try
            {
                var orderDetailsViewModel = new OrderDetailsViewModel
                {
                    OrderId = order.Id,
                    CustomerName = order.Name,
                    CustomerAddress = order.Address,
                    CuAddress = order.CustomerAddress,
                    CustomerPhone = order.PhoneNo,
                    OrderDate = order.OrderDate,
                    Products = order.OrderDetails.Select(od => new ProductViewModel
                    {
                        ProductName = od.Product.Name,
                        Quantity = od.Quantity,
                        Price = od.Product.Price
                    }).ToList(),
                    TotalPrice = order.OrderDetails.Sum(od => od.Quantity * od.Product.Price),

                    // Shop information
                    ShopName = farmerShop.ShopName,
                    ShopId = farmerShop.Id,
                    SpAddress = farmerShop.ShopAddress,

                    // Rider information
                    RiderName = rider?.Name,
                    RiderPhone = rider?.PhoneNumber,
                    RiderId = rider.Id,
                    RpAddress = rider?.RiderAddress,
                    OrderCondition = order.OrderCondition,
                    PayableMoney = delivery?.PayableMoney ?? 0,
                    PaymentMethods = order.PaymentMethods,
                    DelivaryCharge = order.DelivaryCharge,
                    OrderAccept = delivery.OrderAcceptTime
                };

                return View(orderDetailsViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(AllOrdersGroupedByDate));
            }
        }




        [Authorize(Roles = "Farmer")]
        [HttpPost]
        public async Task<IActionResult> ReleaseOrder(int orderId)
        {
            // Retrieve the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            // Retrieve the farmer shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" }); 
            }

            // Get the order and its details
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
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
            var customerUserId = order.UserId; 
            _notificationService.AddNotification(customerUserId, $"Your order {order.Id} is on the way. You can track it.", order.Id);

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

           
            return RedirectToAction("AllOrdersGroupedByDate");
        }



        
        public IActionResult ShowMostSoldProduct()
        {
            return View();
        }

        
    


        public IActionResult CashoutRevenue()
        {
            return View();
        }

        public IActionResult Wallet()
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var farmer = _context.FarmerShop.SingleOrDefault(f => f.FarmerUserId == farmerId);
            return View(farmer);
        }

        public IActionResult RequestWithdraw()
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var farmer = _context.FarmerShop.SingleOrDefault(f => f.FarmerUserId == farmerId);
            return View(farmer);
        }

        [HttpPost]
        public IActionResult RequestWithdraw(decimal amount)
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var farmer = _context.FarmerShop.SingleOrDefault(f => f.FarmerUserId == farmerId);

            if (farmer == null || farmer.ShopRevenue < amount)
            {
                TempData["Error"] = "Insufficient funds.";
                return RedirectToAction("RequestWithdraw");
            }

            var request = new WithdrawalHistory
            {
                UserId = farmerId,
                Amount = amount,
                RequestDate = DateTime.Now,
                IsApproved = false,
                UserType = "Farmer"
            };

            _context.withdrawalHistories.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "Withdrawal request submitted successfully.";
            return RedirectToAction("Wallet");
        }

        [Authorize(Roles = "Rider,Farmer")]
        public async Task<IActionResult> WithdrawHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            var role = await _userManager.GetRolesAsync(user);
            var withdrawHistory = await _context.withdrawalHistories
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.ConfirmDate)
                .ToListAsync();

            return View(withdrawHistory);
        }


        public IActionResult FarmerRevenueDetails(int farmerId)
        {
            var farmer = _context.FarmerShop.SingleOrDefault(f => f.Id == farmerId);
            return View(farmer);
        }








    }
}

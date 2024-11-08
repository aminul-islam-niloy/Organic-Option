using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.Payment;
using OnlineShop.Session;
using OrganicOption.Models;
using OrganicOption.Service;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineShop.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class OrderController : Controller


    {
        private readonly NotificationService _notificationService;
        private readonly StripeSettings _stripeSettings;

        public string SessionId { get; set; }

        private ApplicationDbContext _db;
        UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IOptions<StripeSettings> stripeSettings, NotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;
            _notificationService = notificationService;
        }


        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == userId);

            List<Products> products = HttpContext.Session.Get<List<Products>>("products");
            double totalDeliveryCharge = 0;

            if (products != null)
            {
                var groupedProducts = products.GroupBy(p => p.FarmerShopId);

                foreach (var shopProducts in groupedProducts)
                {
                    // Find the farmer shop
                    var farmerShop = _db.FarmerShop.FirstOrDefault(f => f.Id == shopProducts.Key);
                    if (farmerShop != null)
                    {
                        double maxChargeForShop = 0;

                        foreach (var product in shopProducts)
                        {
                            double distance = CalculateDistance(userInfo.Latitude, userInfo.Longitude, farmerShop.Latitude, farmerShop.Longitude);
                            double baseCharge = CalculateBaseCharge(distance);
                            double additionalCharge = CalculateAdditionalCharge(product);

                            // Calculate the maximum charge for products from this shop
                            maxChargeForShop = Math.Max(maxChargeForShop, baseCharge + additionalCharge);
                        }

                        // Add the highest charge for this shop to the total delivery charge
                        totalDeliveryCharge += maxChargeForShop;
                    }
                }
            }

            ViewBag.TotalDeliveryCharge = totalDeliveryCharge;
            return View();
        }


        private double CalculateAdditionalCharge(Products product)
        {
            double additionalCharge = 0;

            if (product.ProductTypes.ProductType == "Cattle" && product.QuantityType == QuantityType.Item)
            {
                additionalCharge = product.QuantityInCart * 1000;
            }
            else if (product.QuantityType == QuantityType.Item)
            {
                if (product.QuantityInCart <= 5) additionalCharge = 0;
                else if (product.QuantityInCart <= 40) additionalCharge = 50;
                else if (product.QuantityInCart <= 100) additionalCharge = 100;
                else if (product.QuantityInCart <= 500) additionalCharge = 200;
                else if (product.QuantityInCart <= 1000) additionalCharge = 400;
                else additionalCharge = 600;
            }

            else if (product.ProductTypes.ProductType == "Fruits" && product.QuantityType == QuantityType.Kg)
            {
                if (product.QuantityInCart <= 5) additionalCharge = 0;
                else if (product.QuantityInCart <= 40) additionalCharge = 50;
                else if (product.QuantityInCart <= 100) additionalCharge = 100;
                else if (product.QuantityInCart <= 500) additionalCharge = 200;
                else if (product.QuantityInCart <= 1000) additionalCharge = 400;
                else additionalCharge = 600;
            }
            else if (product.QuantityType == QuantityType.Kg)
            {
                if (product.QuantityInCart <= 5) additionalCharge = 0;
                else if (product.QuantityInCart <= 40) additionalCharge = 50;
                else if (product.QuantityInCart <= 100) additionalCharge = 100;
                else if (product.QuantityInCart <= 500) additionalCharge = 200;
                else if (product.QuantityInCart <= 1000) additionalCharge = 400;
                else additionalCharge = 600;
            }
            else if (product.ProductTypes.ProductType == "Dairy" && product.QuantityType == QuantityType.Liter)
            {
                if (product.QuantityInCart <= 5) additionalCharge = 0;
                else if (product.QuantityInCart <= 40) additionalCharge = 50;
                else if (product.QuantityInCart <= 100) additionalCharge = 100;
                else if (product.QuantityInCart <= 500) additionalCharge = 300;
                else if (product.QuantityInCart <= 1000) additionalCharge = 500;
                else additionalCharge = 800;
            }

            return additionalCharge;
        }


        public static double CalculateBaseCharge(double distance)
        {
            if (distance <= 5) return 50;
            if (distance <= 10) return 100;
            if (distance <= 20) return 150;
            if (distance <= 50) return 200;
            return 300;
        }



        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the Earth in kilometers
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distance in kilometers
        }

        private static double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }


        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order anOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);

            anOrder.Latitude = userInfo.Latitude;
            anOrder.Longitude = userInfo.Longitude;
            anOrder.UserId = userId;
            anOrder.OrderDate = DateTime.Now;

            List<Products> products = HttpContext.Session.Get<List<Products>>("products");
            double totalDeliveryCharge = 0;

            if (products != null)
            {
                // Group products by FarmerShopId
                var groupedProducts = products.GroupBy(p => p.FarmerShopId);

                foreach (var shopProducts in groupedProducts)
                {

                    var farmerShop = _db.FarmerShop.FirstOrDefault(f => f.Id == shopProducts.Key);
                    if (farmerShop != null)
                    {
                        double maxChargeForShop = 0;

                        foreach (var product in shopProducts)
                        {
                            double distance = CalculateDistance(userInfo.Latitude, userInfo.Longitude, farmerShop.Latitude, farmerShop.Longitude);
                            double baseCharge = CalculateBaseCharge(distance);
                            double additionalCharge = CalculateAdditionalCharge(product);

                            maxChargeForShop = Math.Max(maxChargeForShop, baseCharge + additionalCharge);

                            var orderDetails = new OrderDetails
                            {
                                PorductId = product.Id,
                                Price = product.Price + (product.Discount > 0 ? product.DiscountPrice : 0),
                                Quantity = product.QuantityInCart,
                                TotalDelivaryCharge = (decimal)(baseCharge + additionalCharge),
                                DiscountedPrice = product.Discount
                            };
                            anOrder.OrderDetails.Add(orderDetails);
                        }
                        totalDeliveryCharge += maxChargeForShop;
                    }
                }
            }

            anOrder.DelivaryCharge = (decimal)totalDeliveryCharge;

            anOrder.OrderNo = GetOrderNo();
            _db.Orders.Add(anOrder);
            await _db.SaveChangesAsync();

            // Notify farmers
            if (products != null)
            {
                foreach (var product in products)
                {
                    UpdateFarmerStore(product.Id, product.QuantityInCart, product.Price, anOrder.Id, product.FarmerShopId);

                    var farmerShop = _db.FarmerShop.Include(f => f.FarmerUser)
                                                   .FirstOrDefault(f => f.Id == product.FarmerShopId);
                    if (farmerShop != null)
                    {
                        string farmerUserId = farmerShop.FarmerUserId;
                        _notificationService.AddNotification(farmerUserId, $"{anOrder.OrderNo} that '{product.Name}' has been ordered from your Shop.", product.Id);
                    }
                }
            }

            HttpContext.Session.Set("products", new List<Products>());
            return RedirectToAction("PaymentPage", new { orderId = anOrder.Id });
        }



        private void UpdateFarmerStore(int id, int quantitySold, decimal price, int orderId, int FarmerShopId)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.SoldQuantity += quantitySold;
                product.LastSoldDate = DateTime.Now;
            }

            FarmerShop farmerStore = _db.FarmerShop
                            .Include(fs => fs.Inventory)
                            .FirstOrDefault(fs => fs.Id == product.FarmerShopId);

            if (farmerStore != null)
            {
                farmerStore.SoldQuantity += quantitySold;
                farmerStore.LastSoldDate = DateTime.Now;

                // Create a new inventory item 
                farmerStore.Inventory.Add(new InventoryItem
                {
                    ProductId = id,
                    Quantity = quantitySold,
                    LastSoldDate = DateTime.Now,
                    Price = price,
                    OrderId = orderId
                });
            }
            _db.SaveChanges();
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        //Success?orderId={orderId}"

        public string GetOrderNo()
        {
            int rowCount = _db.Orders.ToList().Count() + 1;
            return rowCount.ToString("000");
        }


        [Authorize(Roles = "Admin")]

        public IActionResult AllOrders()
        {
            var orders = _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                    .ThenInclude(o => o.User)
                .GroupBy(od => new
                {
                    od.OrderId,
                    od.Order.OrderNo,
                    od.Order.Name,
                    od.Order.Address,
                    od.Order.Email,
                    od.Order.PhoneNo,
                    od.Order.OrderDate,
                    od.Order.UserId,
                    od.Order.User.UserName,
                    od.Order.OrderCondition
                })
                .Select(g => new OrderDetailsViewModel
                {
                    OrderId = g.Key.OrderId,
                    OrderNo = g.Key.OrderNo,
                    CustomerName = g.Key.Name,
                    OrderCondition = g.Key.OrderCondition,
                    CustomerAddress = g.Key.Address,
                    CustomerPhone = g.Key.PhoneNo,
                    CustomerEmail = g.Key.Email,
                    OrderDate = g.Key.OrderDate,
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    PaymentMethods = g.First().PaymentMethods,
                    Products = g.Select(od => new ProductViewModel
                    {
                        ProductId = od.PorductId,
                        ProductName = od.Product.Name,
                        Price = od.Product.Price,
                        Quantity = od.Quantity
                    }).ToList(),
                    TotalPrice = g.Sum(od => od.Product.Price * od.Quantity)
                }).OrderByDescending(g => g.OrderDate).ToList();

            return View(orders);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AllOrders(DateTime? filterDate, string filterType = "current")
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;

            DateTime startDate;
            DateTime endDate;

            if (filterType == "previous")
            {
                // Previous month
                startDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (filterType == "specific" && filterDate.HasValue)
            {
                // Specific date
                startDate = filterDate.Value.Date;
                endDate = filterDate.Value.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                // Current month
                startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }

            //   specified date range
            var orders = _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                    .ThenInclude(o => o.User)
                .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate)
                .GroupBy(od => new
                {
                    od.OrderId,
                    od.Order.OrderNo,
                    od.Order.Name,
                    od.Order.Address,
                    od.Order.Email,
                    od.Order.PhoneNo,
                    od.Order.OrderDate,
                    od.Order.UserId,
                    od.Order.User.UserName,
                    od.Order.OrderCondition
                })
                .Select(g => new OrderDetailsViewModel
                {
                    OrderId = g.Key.OrderId,
                    OrderNo = g.Key.OrderNo,
                    CustomerName = g.Key.Name,
                    OrderCondition = g.Key.OrderCondition,
                    CustomerAddress = g.Key.Address,
                    CustomerPhone = g.Key.PhoneNo,
                    CustomerEmail = g.Key.Email,
                    OrderDate = g.Key.OrderDate,
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    PaymentMethods = g.First().PaymentMethods,
                    Products = g.Select(od => new ProductViewModel
                    {
                        ProductId = od.PorductId,
                        ProductName = od.Product.Name,
                        Price = od.Product.Price,
                        Quantity = od.Quantity
                    }).ToList(),
                    TotalPrice = g.Sum(od => od.Product.Price * od.Quantity)
                }).OrderByDescending(g => g.OrderDate).ToList();

            return View(orders);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult OrderDetails(int id)
        {

            var order = _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                    .ThenInclude(o => o.User)
                .Where(od => od.OrderId == id)
                .Select(od => new OrderDetailsViewModel
                {
                    OrderId = od.OrderId,
                    OrderNo = od.Order.OrderNo,
                    CustomerName = od.Order.Name,
                    CustomerAddress = od.Order.Address,
                    CustomerPhone = od.Order.PhoneNo,
                    CustomerEmail = od.Order.Email,
                    ShopId = od.Product.FarmerShopId,
                    //RiderId= Rider.
                    CardInfo = od.StripeSessionId,
                    OrderDate = od.Order.OrderDate,
                    UserId = od.Order.UserId,
                    UserName = od.Order.User.UserName,
                    PaymentMethods = od.PaymentMethods,
                    Products = new List<ProductViewModel>
                    {
                new ProductViewModel
                {
                    ProductId = od.Id,
                    ProductName = od.Product.Name,
                    Price = od.Product.Price,
                    Quantity = od.Quantity
                }
                    },
                    TotalPrice = od.Product.Price * od.Quantity,
                    TotalDelivaryCharge = od.TotalDelivaryCharge,
                    TotalDiscount = od.DiscountedPrice
                }).FirstOrDefault();

            if (order == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }


            //   Rider information related  order
            var rider = (from d in _db.Deliveries
                         join r in _db.RiderModel on d.RiderId equals r.Id
                         where d.OrderId == id
                         select new
                         {
                             Id = r.Id,
                             RiderName = r.Name,
                             RiderPhone = r.PhoneNumber

                         }).FirstOrDefault();


            ViewBag.RiderId = rider?.Id;
            ViewBag.RiderName = rider?.RiderName;
            ViewBag.RiderPhone = rider?.RiderPhone;


            return View(order);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult CardOrders()
        {

            var cardOrders = _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.PaymentMethods == PaymentMethods.Card)
                   .OrderByDescending(od => od.Id).ToList();

            if (cardOrders == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            return View(cardOrders);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult CashOrders()
        {

            var cashOrders = _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.PaymentMethods == PaymentMethods.CashOnDelivery)
                 .OrderByDescending(od => od.Id).ToList();

            if (cashOrders == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            return View(cashOrders);
        }




        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _db.OrderDetails.FindAsync(id);
            if (order == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            _db.OrderDetails.Remove(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("AllOrders");
        }



        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);
            var userOrdersWithProducts = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            // Map the data to view model
            var viewModel = userOrdersWithProducts.Select(order => new UserOrdersViewModel
            {
                UserName = user.UserName,
                UserPhone = user.PhoneNumber,
                OrderNo = order.OrderNo,
                OrderDate = order.OrderDate,
                OrderCondition = order.OrderCondition,
                DeliveryCharge = (double)order.DelivaryCharge,
                TotalPrice = order.OrderDetails.Sum(od => od.Quantity * od.Price) + order.DelivaryCharge,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailsViewModel
                {
                    ProductId = od.PorductId,
                    ProductName = od.Product.Name,
                    ProductImage = od.Product.Image,
                    ShopId = od.Product.FarmerShopId,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    PaymentMethods = od.PaymentMethods,

                }).ToList(),
                ShopId = order.OrderDetails.FirstOrDefault().Product.FarmerShopId
            }).OrderByDescending(g => g.OrderDate).ToList();

            return View(viewModel);
        }


        public IActionResult OrderTracking()
        {
            return View();
        }


        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserOrder(string id)
        {

            int orderId;
            if (!int.TryParse(id, out orderId))
            {
                return BadRequest();
            }

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNo == id);
            if (order == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(UserOrders));
        }


        //stripe payment method

        [HttpPost]
        public IActionResult CreatePayment(int orderId)
        {
            var order = _db.Orders.Include(o => o.OrderDetails)
                                  .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var Amount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
            var DelivaryAmount = order.DelivaryCharge;
            var totalAmount = Amount + DelivaryAmount;

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            // Create the Stripe session
            var sessionService = new SessionService();
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(totalAmount),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Order Payment"
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = "https://localhost:44343/User/Deshboard/Index",
                CancelUrl = "https://localhost:44343/User/Deshboard/Index"
            };


            var session = sessionService.Create(options);

            // Store the session ID in the order details
            order.OrderDetails.ForEach(od =>
            {
                od.StripeSessionId = session.Id;
                od.PaymentMethods = PaymentMethods.Card;
            });


            _db.SaveChanges();


            return Redirect(session.Url);
        }


        //  PaymentPage

        public IActionResult PaymentPage(int orderId)
        {
            // Pass orderId to the view
            ViewBag.OrderId = orderId;
            return View();
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisplayOrdersOnList()
        {

            var ordersOnList = await _db.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist))
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            return View(ordersOnList);
        }


    }

}




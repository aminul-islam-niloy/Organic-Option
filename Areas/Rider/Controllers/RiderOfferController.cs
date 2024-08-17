using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models;
using OrganicOption.Models.Rider_Section;
using OrganicOption.Service;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using OnlineShop.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Session;
using OrganicOption.Models;

namespace OrganicOption.Areas.Rider.Controllers
{
    [Area("Rider")]
    public class RiderOfferController : Controller
    {
        private readonly ApplicationDbContext _db;
        UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;
        private readonly NotificationService _notificationService;

        public RiderOfferController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _db = db;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cache = memoryCache;
        }


        [Authorize(Roles = "Rider")]
        public async Task<IActionResult> MyOffer()
        {
            var offer = await GetOfferForRider();

            if (offer != null)
            {
                var viewModel = new RiderOfferViewModel
                {
                    OrderId = offer.OrderId,
                    ProductDetails = offer.ProductDetails,
                    CustomerAddress = offer.CustomerAddress,
                    CustomerPhone = offer.CustomerPhone,
                    DeliveryTime = offer.DeliveryTime,
                    ShopName = offer.ProductDetails.FirstOrDefault()?.ShopName,
                    ShopContract = offer.ProductDetails.FirstOrDefault()?.ShopContact,
                    Revenue = offer.Revenue,
                    letetude = offer.letetude,
                    longatude = offer.longatude,
                    ShopAddress = offer.ShopAddress,
                    TimeRemaining = (offer.OfferStartTime.AddMinutes(1) - DateTime.Now)
                };

                HttpContext.Session.Set("OfferData", viewModel);

                return View(viewModel);
            }
            else
            {
                return Content("No offer available.");
            }
        }


        //  index of the next order
        private static int nextOrderIndex = 0;


        [Authorize(Roles = "Rider")]
        private async Task<RiderOfferViewModel> GetOfferForRider()
        {
            var availableRider = await FindAvailableRider();

            if (availableRider != null)
            {
                var ordersOnList = await _db.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.FarmerShop)
                    .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist) && !o.IsOfferedToRider)
                    .OrderBy(o => o.Id)
                    .ToListAsync();

                // Check if there are any orders available
                if (ordersOnList.Count > 0)
                {
                    // Get the order based on the nextOrderIndex
                    var order = ordersOnList.ElementAtOrDefault(nextOrderIndex);

                    var farmerShopId = order.OrderDetails
                        .Select(od => od.Product.FarmerShopId)
                        .FirstOrDefault();

                    var shopAddress = await _db.FarmerShop
                        .Where(fs => fs.Id == farmerShopId)
                        .Select(fs => fs.ShopAddress)
                        .FirstOrDefaultAsync();

                    var GeoLocation = await _db.FarmerShop.Where(fs => fs.Id == farmerShopId).FirstOrDefaultAsync();

                    if (order != null)
                    {
                        var productDetails = order.OrderDetails.Select(od => new ProductwithOrderViewModel
                        {
                            ProductName = od.Product.Name,
                            ProductImage = od.Product.Image,
                            ShopName = od.Product.FarmerShop.ShopName,
                            ShopContact = od.Product.FarmerShop.ContractInfo,
                            Quantity = od.Quantity,
                            ShopAddress = od.Product.FarmerShop.ShopAddress
                        }).ToList();

                        var offerViewModel = new RiderOfferViewModel
                        {
                            OrderId = order.Id,
                            CustomerAddress = order.Address,
                            CutomerCurrentAddress = order.CustomerAddress,
                            DeliveryTime = EstimateDeliveryTime(order),
                            Revenue = CalculateEarnings(order),
                            letetude = GeoLocation.Latitude,
                            longatude = GeoLocation.Longitude,
                            ProductDetails = productDetails,
                            CustomerPhone = order.PhoneNo,
                            ShopAddress = shopAddress,
                            OfferStartTime = DateTime.Now
                        };

                        // Increment the nextOrderIndex for the next call
                        nextOrderIndex = (nextOrderIndex + 1) % ordersOnList.Count;

                        return offerViewModel;
                    }
                }
            }

            return null;
        }

        private async Task<RiderModel> FindAvailableRider()
        {
            var availableRider = await _db.RiderModel
                .FirstOrDefaultAsync(r => r.RiderStatus && !r.OnDeliaryByOffer);

            return availableRider;
        }



        private TimeSpan EstimateDeliveryTime(Order order)
        {
            //  Fixed average delivery time of 30 minutes
            return TimeSpan.FromMinutes(30);
        }

        private decimal CalculateEarnings(Order order)
        {
            //// Example: Earnings are 10% of the total order amount
            //decimal totalOrderAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
            //decimal earningsPercentage = 0.02m; // 2%
            //return totalOrderAmount * earningsPercentage;

            decimal deliveryCharge = order.DelivaryCharge; // Assuming DelivaryCharge is the delivery charge for the order

            if (deliveryCharge < 50)
            {
                deliveryCharge = 70;
            }

            // Calculate earnings as 70% of the delivery charge
            decimal earnings = deliveryCharge * 0.70m;

            return earnings;

        }




        [Authorize(Roles = "Rider")]
        public async Task<IActionResult> CreateDeliveryForAcceptedOrder()
        {
            var riderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingRider = await _db.RiderModel.FirstOrDefaultAsync(rider => rider.RiderUserId == riderId);

            // Retrieve offer data from session
            var offer = HttpContext.Session.Get<RiderOfferViewModel>("OfferData");

            if (offer != null)
            {
                var order = await _db.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.FarmerShop)
                    .FirstOrDefaultAsync(o => o.Id == offer.OrderId);

                if (order == null)
                {
                    return View("Error");
                }

                decimal totalOrderAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);


                bool isPaymentByCard = order.OrderDetails.Any(od => od.PaymentMethods == PaymentMethods.Card) || order.PaymentMethods == PaymentMethods.Card;

                var delivery = new Delivery
                {
                    OrderId = offer.OrderId,
                    RiderId = existingRider.Id,
                    OrderCondition = OrderCondition.OrderTaken,
                    PayableMoney = isPaymentByCard ? 0 : totalOrderAmount,
                    ProductDetails = string.Join(", ", offer.ProductDetails.Select(product => product.ProductName)),
                    CustomerAddress = offer.CustomerAddress,
                    DelivyAddress = offer.CustomerAddress,
                    CustomerPhone = offer.CustomerPhone,
                    ShopName = offer.ShopName,
                    ShopContract = offer.ShopContract,
                    ShopLat = offer.letetude,
                    ShopLon = offer.longatude,
                    DeliveryLat = order.Latitude,
                    DeliveryLon = order.Longitude,
                    OrderAcceptTime = DateTime.Now
                };

                try
                {
                    foreach (var orDesin in order.OrderDetails)
                    {
                        orDesin.OrderCondition = OrderCondition.OrderTaken;

                    }
                    order.IsOfferedToRider = true;

                    order.OrderCondition = OrderCondition.OrderTaken;
                    _db.Deliveries.Add(delivery);
                    await _db.SaveChangesAsync();

                    foreach (var orderDetail in order.OrderDetails)
                    {
                        var product = orderDetail.Product;
                        var farmerShop = _db.FarmerShop.Include(f => f.FarmerUser)
                                                        .FirstOrDefault(f => f.Id == product.FarmerShopId);
                        if (farmerShop != null)
                        {
                            string farmerUserId = farmerShop.FarmerUserId;
                            _notificationService.AddNotification(farmerUserId, $"Order #{order.Id} has been accepted by a rider {existingRider.Id}. Product: '{product.Name}'", product.Id);
                        }
                    }

                    ViewBag.ShopAddress = offer.ShopAddress;
                    HttpContext.Session.Remove("OfferData");

                    return View(delivery);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return RedirectToAction("Index", "RiderDelivery", new { area = "Rider" });
                }
            }
            else
            {
                return RedirectToAction("Index", "RiderDelivery", new { area = "Rider" });
            }
        }





    }
}

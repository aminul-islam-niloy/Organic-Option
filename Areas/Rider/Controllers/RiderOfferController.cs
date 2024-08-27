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

        public RiderOfferController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache, NotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cache = memoryCache;
            _notificationService = notificationService;
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
                    CustomerName = offer.CustomerName,
                    CutomerCurrentAddress = offer.CutomerCurrentAddress,
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
                return RedirectToAction("Index", "RiderDelivery", new { area = "Rider" });
            }
        }


        //  index of the next order
        private static int nextOrderIndex = 0;


        [Authorize(Roles = "Rider")]
        private async Task<RiderOfferViewModel> GetOfferForRider()
        {
            // orders that are on the list and not yet offered to a rider
            var ordersOnList = await _db.Orders
                .Include(o => o.CustomerAddress)
                   .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.FarmerShop)
                .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist) && !o.IsOfferedToRider)
                .OrderBy(o => o.Id)
                .ToListAsync();

            if (ordersOnList.Count > 0)
            {
                // Get the order based on the nextOrderIndex
                var order = ordersOnList.ElementAtOrDefault(nextOrderIndex);

                if (order != null)
                {
                    // Retrieve the FarmerShop 
                    var farmerShopId = order.OrderDetails
                        .Select(od => od.Product.FarmerShopId)
                        .FirstOrDefault();

                    var shopLocation = await _db.FarmerShop.Include(fs =>fs.ShopAddress)
                        .Where(fs => fs.Id == farmerShopId)
                        .Select(fs => new { fs.Latitude, fs.Longitude, fs.ShopAddress })
                        .FirstOrDefaultAsync();

                    if (shopLocation != null)
                    {
                        
                        var availableRider = await FindAvailableRider(shopLocation.Latitude, shopLocation.Longitude);

                        if (availableRider != null)
                        {
                            var distance = CalculateDistance(shopLocation.Latitude, shopLocation.Longitude, order.Latitude, order.Longitude);

                            var productDetails = order.OrderDetails.Select(od => new ProductwithOrderViewModel
                            {
                                ProductName = od.Product.Name,
                                ProductImage = od.Product.Image,
                                ShopName = od.Product.FarmerShop.ShopName,
                                ShopContact = od.Product.FarmerShop.ContractInfo,
                                Quantity = od.Quantity,
                                ShopAddress = shopLocation.ShopAddress
                            }).ToList();

                            var offerViewModel = new RiderOfferViewModel
                            {
                                OrderId = order.Id,
                                CustomerName = order.Name,
                                CustomerAddress = order.Address,
                                CutomerCurrentAddress = order.CustomerAddress, //address address
                                DeliveryTime = EstimateDeliveryTime(distance),
                                Revenue = order.DelivaryCharge,
                                letetude = shopLocation.Latitude,
                                longatude = shopLocation.Longitude,
                                ProductDetails = productDetails,
                                CustomerPhone = order.PhoneNo,
                                ShopAddress = shopLocation.ShopAddress,
                                OfferStartTime = DateTime.Now
                            };

                            // Increment the nextOrderIndex for the next call
                            nextOrderIndex = (nextOrderIndex + 1) % ordersOnList.Count;

                            return offerViewModel;
                        }
                    }
                }
            }

            return null;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Earth's radius in meters
            var φ1 = lat1 * Math.PI / 180; // φ, λ in radians
            var φ2 = lat2 * Math.PI / 180;
            var Δφ = (lat2 - lat1) * Math.PI / 180;
            var Δλ = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = R * c; //  meters
            return distance / 1000; //  km
        }

        // available rider within 115 km of the FarmerShop
        private async Task<RiderModel> FindAvailableRider(double shopLat, double shopLon)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == currentUser.Id);
           

            double Latitude = user.Latitude;
            double Longitude = user.Longitude;

            var riders = await _db.RiderModel
                .Where(r => r.RiderStatus && !r.OnDeliaryByOffer)
                .ToListAsync();

            foreach (var rider in riders)
            {
                var distance = CalculateDistance(shopLat, shopLon, Latitude, Longitude);
                if (distance <= 150)
                {
                    return rider;
                }
            }

            return null;
        }


        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

    
        private TimeSpan EstimateDeliveryTime(double distanceKm)
        {
            double averageSpeedKmph = 30.0;
            double timeInMinutes = (distanceKm / averageSpeedKmph) * 60;

            // Add a buffer time 
            timeInMinutes += 7.0;

            return TimeSpan.FromMinutes(timeInMinutes);
        }

  
        private decimal CalculateEarnings(Order order, double distanceKm)
        {
            decimal baseDeliveryCharge = CalculateBaseDeliveryCharge(distanceKm);
            decimal additionalProductCharge = CalculateProductBasedCharge(order);

            decimal totalCharge = baseDeliveryCharge + additionalProductCharge;

        
           // decimal earnings = totalCharge * 0.70m; // 70% to the rider

            return totalCharge;
        }

        // delivery charge based on distance
        private decimal CalculateBaseDeliveryCharge(double distanceKm)
        {
            if (distanceKm <= 5)
            {
                return 50m;
            }
            else if (distanceKm <= 10)
            {
                return 100m;
            }
            else if (distanceKm <= 20)
            {
                return 150m;
            }
            else if (distanceKm <= 50)
            {
                return 200m;
            }
            else
            {
                return 300m;
            }
        }

        //  additional charges based on the product type and quantity
        private decimal CalculateProductBasedCharge(Order order)
        {
            decimal additionalCharge = 0m;

            foreach (var orderDetail in order.OrderDetails)
            {
                if (orderDetail?.Product?.ProductTypes == null)
                {
                    //  skip to the next iteration
                    continue;
                }

                additionalCharge += CalculateAdditionalCharge(orderDetail.Product, orderDetail.Quantity);
            }

            return additionalCharge;
        }

        // calculate additional charge based on product type and quantity
        private decimal CalculateAdditionalCharge(Products product, decimal quantity)
        {
            switch (product.ProductTypes.ProductType)
            {
                case "Cattle":
                    return quantity * 1000m; // Per cattle

                case "Crops":
                    return CalculateCropsCharge(quantity);

                case "Liquid":
                    return CalculateLiquidCharge(quantity);

                default:
                    return 0m; 
            }
        }

        // additional charge for crops based on quantity (in Kg)
        private decimal CalculateCropsCharge(decimal quantityKg)
        {
            if(quantityKg <= 10)
            {
                return 0m;
            }
            else if (quantityKg <= 50)
            {
                return 50m;
            }
            else if (quantityKg <= 100)
            {
                return 100m;
            }
            else if (quantityKg <= 500)
            {
                return 200m;
            }
            else if (quantityKg <= 1000)
            {
                return 400m;
            }
            else
            {
                return 600m;
            }
        }

        //  charge for liquids based on quantity (in Liters)
        private decimal CalculateLiquidCharge(decimal quantityLiters)
        {
            if(quantityLiters <= 10)
            {
                return 0m;
            }
            else if (quantityLiters <= 50)
            {
                return 50m;
            }
            else if (quantityLiters <= 100)
            {
                return 100m;
            }
            else if (quantityLiters <= 500)
            {
                return 300m;
            }
            else if (quantityLiters <= 1000)
            {
                return 500m;
            }
            else
            {
                return 800m;
            }
        }




        [Authorize(Roles = "Rider")]
        public async Task<IActionResult> CreateDeliveryForAcceptedOrder()
        {
            var riderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingRider = await _db.RiderModel.FirstOrDefaultAsync(rider => rider.RiderUserId == riderId);

            var offer = HttpContext.Session.Get<RiderOfferViewModel>("OfferData");

            if (offer != null)
            {
                var order = await _db.Orders
                    .Include(o => o.CustomerAddress)
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
                    PayableMoney = isPaymentByCard ? 0 : totalOrderAmount + offer.Revenue,
                    ProductDetails = string.Join(", ", offer.ProductDetails.Select(product => product.ProductName)),
                    CustomerAddress = offer.CustomerAddress,
                    DelivyAddress = offer.CustomerAddress,
                    //CustomerCurrentAddress = offer.CutomerCurrentAddress,
                    CustomerPhone = offer.CustomerPhone,
                    ShopName = offer.ShopName,
                    ShopContract = offer.ShopContract,
                    //ShopCurrentAddress = offer.ShopAddress,
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

                    var rider = _db.RiderModel.SingleOrDefault(r => r.RiderUserId == riderId);
                    order.IsOfferedToRider = true;
                    rider.OnDeliaryByOffer = true;  
                    order.OrderCondition = OrderCondition.OrderTaken;
                    //delivery.ShopCurrentAddress = offer.ShopAddress;
                    //delivery.CustomerCurrentAddress = offer.CutomerCurrentAddress;

                    _db.Deliveries.Add(delivery);
                    await _db.SaveChangesAsync();
                    ViewBag.ShopAddress = offer.ShopAddress;
                    ViewBag.CustomerAddress= offer.CutomerCurrentAddress;
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

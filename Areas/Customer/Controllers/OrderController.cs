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
using OrganicOption.Models.Rider_Section;
using OrganicOption.Models.View_Model;
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






        //calculate Distance and Charge
    

        public static double CalculateBaseCharge(double distance)
        {
            if (distance <= 5) return 50;
            if (distance <= 10) return 100;
            if (distance <= 20) return 150;
            if (distance <= 50) return 200;
            return 300;
        }

      



        //GET Checkout actioin method

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
                foreach (var product in products)
                {
                    var farmerShop = _db.FarmerShop.FirstOrDefault(f => f.Id == product.FarmerShopId);
                    if (farmerShop != null)
                    {
                        double distance = CalculateDistance(userInfo.Latitude, userInfo.Longitude, farmerShop.Latitude, farmerShop.Longitude);
                        double baseCharge = CalculateBaseCharge(distance);
                        double additionalCharge = CalculateAdditionalCharge(product);
                        totalDeliveryCharge += baseCharge + additionalCharge;
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
            else if (product.ProductTypes.ProductType == "Crops" && product.QuantityType == QuantityType.Kg)
            {
                if (product.QuantityInCart <= 50) additionalCharge = 50;
                else if (product.QuantityInCart <= 100) additionalCharge = 100;
                else if (product.QuantityInCart <= 500) additionalCharge = 200;
                else if (product.QuantityInCart <= 1000) additionalCharge = 400;
                else additionalCharge = 600;
            }
            else if (product.ProductTypes.ProductType == "Liquid" && product.QuantityType == QuantityType.Liter)
            {
                if (product.QuantityInCart <= 50) additionalCharge = 50;
                else if (product.QuantityInCart <= 100) additionalCharge = 100;
                else if (product.QuantityInCart <= 500) additionalCharge = 300;
                else if (product.QuantityInCart <= 1000) additionalCharge = 500;
                else additionalCharge = 800;
            }

            return additionalCharge;
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




        //public IActionResult Checkout()
        //{
        //    return View();
        //}

        //POST Checkout action method

        //[Authorize(Roles = "Customer")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Checkout(Order anOrder)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        //    var user = await _userManager.FindByIdAsync(userId);


        //    var currentUser = await _userManager.GetUserAsync(User);

        //    var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);


        //    anOrder.Latitude = userInfo.Latitude;
        //    anOrder.Longitude = userInfo.Longitude;
        //    anOrder.UserId = userId;
        //    anOrder.OrderDate = DateTime.Now;


        //    List<Products> products = HttpContext.Session.Get<List<Products>>("products");
        //    if (products != null)
        //    {
        //        foreach (var product in products)
        //        {
        //            var orderDetails = new OrderDetails
        //            {
        //                PorductId = product.Id,
        //                Price = product.Price + (product.Discount > 0 ? product.DiscountPrice : 0),
        //                Quantity = product.QuantityInCart
        //            };
        //            anOrder.OrderDetails.Add(orderDetails);
        //        }
        //    }

        //    anOrder.OrderNo = GetOrderNo();
        //    _db.Orders.Add(anOrder);
        //    await _db.SaveChangesAsync();

        //    List<Products> sesProducts = HttpContext.Session.Get<List<Products>>("products");
        //    if (sesProducts != null)
        //    {
        //        foreach (var product in sesProducts)
        //        {
        //            UpdateFarmerStore(product.Id, product.QuantityInCart, product.Price, anOrder.Id, product.FarmerShopId);

        //            // Retrieve FarmerUserId based on FarmerShopId
        //            var farmerShop = _db.FarmerShop.Include(f => f.FarmerUser)
        //                                            .FirstOrDefault(f => f.Id == product.FarmerShopId);
        //            if (farmerShop != null)
        //            {
        //                string farmerUserId = farmerShop.FarmerUserId;
        //                _notificationService.AddNotification(farmerUserId, $"{anOrder.OrderNo} that '{product.Name}'  has been ordered from your Shop.", product.Id);
        //            }
        //        }
        //    }

        //    HttpContext.Session.Set("products", new List<Products>());
        //    return RedirectToAction("PaymentPage", new { orderId = anOrder.Id });
        //}



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
                foreach (var product in products)
                {
                  

                    var farmerShop = _db.FarmerShop.FirstOrDefault(f => f.Id == product.FarmerShopId);
                    if (farmerShop != null)
                    {
                        double distance = CalculateDistance(userInfo.Latitude, userInfo.Longitude, farmerShop.Latitude, farmerShop.Longitude);
                        double baseCharge = CalculateBaseCharge(distance);
                        double additionalCharge = CalculateAdditionalCharge(product);
                        totalDeliveryCharge += baseCharge + additionalCharge;
                    }

                    var orderDetails = new OrderDetails
                    {
                        PorductId = product.Id,
                        Price = product.Price + (product.Discount > 0 ? product.DiscountPrice : 0),
                        Quantity = product.QuantityInCart,
                        TotalDelivaryCharge = (decimal)totalDeliveryCharge,
                        DiscountedPrice=product.Discount
                    };
                    anOrder.OrderDetails.Add(orderDetails);
                }
            }

            anOrder.DelivaryCharge = (decimal)totalDeliveryCharge;

            anOrder.OrderNo = GetOrderNo();
            _db.Orders.Add(anOrder);
            await _db.SaveChangesAsync();

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
            // Retrieve the product and update its sold quantity and last sold date
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.SoldQuantity += quantitySold;
                product.LastSoldDate = DateTime.Now;
            }

            // Retrieve the farmer store associated with the product
            FarmerShop farmerStore = _db.FarmerShop
                            .Include(fs => fs.Inventory) // Include Inventory to access inventory items
                            .FirstOrDefault(fs => fs.Id == product.FarmerShopId);

            // Update the sold quantity and last sold date for the product in the farmer's store
            if (farmerStore != null)
            {
                farmerStore.SoldQuantity += quantitySold;
                farmerStore.LastSoldDate = DateTime.Now;

                // Create a new inventory item 
                farmerStore.Inventory.Add(new InventoryItem
                {
                    ProductId = id,
                    Quantity = quantitySold, // Negative quantity indicates sold
                    LastSoldDate = DateTime.Now,
                    Price = price,
                    OrderId = orderId // Set the OrderId for the inventory item
                });
            }

            // Save changes to the database
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
        public IActionResult Index()
        {
            var ordersWithProductsAndUsers = _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                    .ThenInclude(o => o.User) // Include user details
                .Where(od => od.Order != null && od.Product != null)
                .GroupBy(od => new
                {
                    od.OrderId,
                    od.Order.OrderNo,
                    od.Order.Name,
                    od.Order.Address,
                    od.Order.Email,
                    od.Order.PhoneNo,
                    od.Order.OrderDate,
                    od.Order.UserId, // Include user ID in grouping
                    od.Order.User.UserName,

                   
                })
                .Select(g => new OrderDetailsViewModel
                {
                    OrderId = g.Key.OrderId,
                    OrderNo = g.Key.OrderNo,
                    CustomerName = g.Key.Name,
                    CustomerAddress = g.Key.Address,
                    CustomerPhone = g.Key.PhoneNo,
                    CustomerEmail = g.Key.Email,
                    OrderDate = g.Key.OrderDate,
                    UserId = g.Key.UserId, // Add user ID to the view model
                    UserName = g.Key.UserName,
                    PaymentMethods = g.First().PaymentMethods,


                    Products = g.Select(od => new ProductViewModel
                    {
                        ProductId = od.PorductId,
                        ProductName = od.Product.Name,
                        Price = od.Product.Price,
                        Image = od.Product.Image,

                        Quantity = od.Quantity // Get quantity from OrderDetails
                    }).ToList(),

                    // Calculate the total price by summing the prices of all products in the order
                    TotalPrice = g.Sum(od => od.Product.Price * od.Quantity)
                }).ToList();

            return View(ordersWithProductsAndUsers);
        }

        [HttpGet]
        public IActionResult OrderIndex()
        {
            string dateRange = null;
            // Call the model method with the dateRange
            return await OrderIndexModel(dateRange);
        }

       

        [HttpPost]

        //[Authorize(Roles = "Admin")]

        public async Task<IActionResult> OrderIndexModel(string dateRange)
        {
            DateTime startDate, endDate;
            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split('-');
                startDate = DateTime.Parse(dates[0]);
                endDate = DateTime.Parse(dates[1]);
            }
            else
            {
                startDate = DateTime.Now.AddDays(-7);
                endDate = DateTime.Now;
            }

            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            // Get the Delivery records related to these orders
            var orderIds = orders.Select(o => o.Id).ToList();
            var deliveries = await _db.Deliveries
                .Where(d => orderIds.Contains(d.OrderId))
                .ToListAsync();

            var riderIds = deliveries.Select(d => d.RiderId).Distinct().ToList();
            var riderDict = await _db.RiderModel
                .Where(r => riderIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id);

            var farmerShopIds = orders.SelectMany(o => o.OrderDetails.Select(od => od.Product.FarmerShopId)).Distinct().ToList();
            var farmerShopDict = await _db.FarmerShop
                .Where(fs => farmerShopIds.Contains(fs.Id))
                .ToDictionaryAsync(fs => fs.Id);

            var viewModel = orders.Select(order => new AllOrderViewModel
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                OrderDate = order.OrderDate,
                OrderCondition = order.OrderCondition,
                DeliveryCharge = (double)order.DelivaryCharge,
                TotalPrice = (double)(order.OrderDetails.Sum(od => od.Quantity * od.Price) + order.DelivaryCharge),
                Customer = new CustomerInfoViewModel { Name = order.User.UserName, Phone = order.User.PhoneNumber },
                Rider = deliveries.Where(d => d.OrderId == order.Id)
                          .Select(d => riderDict.ContainsKey(d.RiderId) ? new RiderInfoViewModel
                          {
                              Name = riderDict[d.RiderId].Name,
                              Phone = riderDict[d.RiderId].PhoneNumber
                          } : null)
                          .FirstOrDefault(),
                FarmerShop = (order.OrderDetails.FirstOrDefault()?.Product.FarmerShopId != null
                      && farmerShopDict.ContainsKey(order.OrderDetails.FirstOrDefault().Product.FarmerShopId))
                      ? new FarmerShopInfoViewModel
                      {
                          ShopId = farmerShopDict[order.OrderDetails.FirstOrDefault().Product.FarmerShopId].Id,
                          ShopName = farmerShopDict[order.OrderDetails.FirstOrDefault().Product.FarmerShopId].ShopName
                      } : null,
                Payment = new PaymentInfoViewModel { PaymentMethods = order.PaymentMethods }
            }).ToList();

            return View(viewModel);
        }





        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _db.OrderDetails.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _db.OrderDetails.Remove(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index"); // Redirect to the index page after deletion
        }



        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UserOrders()
        {
            // Retrieve the user ID of the current authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve user details
            var user = await _userManager.FindByIdAsync(userId);

            // Retrieve orders associated with the user's ID including order details and products
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
                DeliveryCharge= (double)order.DelivaryCharge,
                TotalPrice = order.OrderDetails.Sum(od => od.Quantity * od.Price)+order.DelivaryCharge,
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
            }).ToList();

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
                // Handle invalid id format
                return BadRequest();
            }

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNo == id);
            if (order == null)
            {
                return NotFound();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(UserOrders));
        }


        //stripe payment method

        //public IActionResult CreatePayment(string amount)
        //{

        //    var currency = "usd"; // Currency code
        //    var successUrl = "https://localhost:44343/Customer/Order";
        //    var cancelUrl = "https://localhost:44343/Customer/Order/Checkout";
        //    StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

        //    var options = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string>
        //        {
        //            "card"
        //        },
        //        LineItems = new List<SessionLineItemOptions>
        //        {
        //            new SessionLineItemOptions
        //            {
        //                PriceData = new SessionLineItemPriceDataOptions
        //                {
        //                    Currency = currency,
        //                    UnitAmount = Convert.ToInt32(amount) * 100,  // Amount in smallest currency unit (e.g., cents)
        //                    ProductData = new SessionLineItemPriceDataProductDataOptions
        //                    {
        //                        Name = "Product Name",
        //                        Description = "Product Description"
        //                    }
        //                },
        //                Quantity = 1
        //            }
        //        },
        //        Mode = "payment",
        //        SuccessUrl = successUrl,
        //        CancelUrl = cancelUrl
        //    };

        //    var service = new SessionService();
        //    var session = service.Create(options);
        //    SessionId = session.Id;

        //    return Redirect(session.Url);
        //}

        //public async Task<IActionResult> success()
        //{

        //    return View("Index");
        //}

        //public IActionResult cancel()
        //{
        //    return View();
        //}


        [HttpPost]
        public IActionResult CreatePayment(int orderId)
        {
            // Retrieve the order from the database
            var order = _db.Orders.Include(o => o.OrderDetails)
                                  .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var totalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);

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
                SuccessUrl = "https://localhost:44343/Customer/Order/UserOrders",
                CancelUrl = "https://localhost:44343/Customer/Order/PaymentPage"
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





        //[Authorize(Roles = "Rider")]
        //public async Task<IActionResult> MyOffer()
        //{
        //    var offer = await GetOfferForRider();

        //    //HttpContext.Session.Set("OfferData", offer);

        //    if (offer != null)
        //    {
        //        var viewModel = new RiderOfferViewModel
        //        {
        //            OrderId = offer.OrderId,
        //            ProductDetails = offer.ProductDetails,
        //            CustomerAddress = offer.CustomerAddress,
        //            DeliveryTime = offer.DeliveryTime,
        //            ShopName = offer.ProductDetails.FirstOrDefault()?.ShopName, // Get shop name
        //            ShopContract = offer.ProductDetails.FirstOrDefault()?.ShopContact, // Get shop contract
        //            Revenue = offer.Revenue,
        //            ShopAddress= offer.ShopAddress,
        //            TimeRemaining = (offer.OfferStartTime.AddMinutes(1) - DateTime.Now)
        //        };

        //        HttpContext.Session.Set("OfferData", viewModel);

        //        return View(viewModel);
        //    }
        //    else
        //    {
        //        return Content("No offer available.");
        //    }
        //}



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
                    DeliveryTime = offer.DeliveryTime,
                    ShopName = offer.ProductDetails.FirstOrDefault()?.ShopName, // Get shop name
                    ShopContract = offer.ProductDetails.FirstOrDefault()?.ShopContact, // Get shop contract
                    Revenue = offer.Revenue,
                    letetude = offer.letetude,
                    longatude = offer.longatude,
                    ShopAddress = offer.ShopAddress, // Set the ShopAddress
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



        // Define a static variable to keep track of the index of the next order
        private static int nextOrderIndex = 0;


        [Authorize(Roles = "Rider")]
        private async Task<RiderOfferViewModel> GetOfferForRider()
        {
            var availableRider = await FindAvailableRider();

            if (availableRider != null)
            {
                var ordersOnList = await _db.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product) // Include product information
                            .ThenInclude(p => p.FarmerShop) // Include shop information
                    .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist) && !o.IsOfferedToRider)
                    .OrderBy(o => o.Id) // Ensure order is fetched in ascending order of ID
                    .ToListAsync();

                // Check if there are any orders available
                if (ordersOnList.Count > 0)
                {
                    // Get the order based on the nextOrderIndex
                    var order = ordersOnList.ElementAtOrDefault(nextOrderIndex);
                    // Fetch the FarmerShopId from the first order detail
                    var farmerShopId = order.OrderDetails
                        .Select(od => od.Product.FarmerShopId)
                        .FirstOrDefault();

                    // Query to get the FarmerShop address using the FarmerShopId
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
                            ProductImage = od.Product.Image, // Set the product image URL here
                            ShopName = od.Product.FarmerShop.ShopName,
                            ShopContact = od.Product.FarmerShop.ContractInfo,
                            Quantity = od.Quantity,
                            ShopAddress = od.Product.FarmerShop.ShopAddress // Include the ShopAddress
                        }).ToList();



                        var offerViewModel = new RiderOfferViewModel
                        {
                            OrderId = order.Id,
                            CustomerAddress = order.Address,
                            DeliveryTime = EstimateDeliveryTime(order),
                            Revenue = CalculateEarnings(order),
                            letetude = GeoLocation.Latitude,
                            longatude = GeoLocation.Longitude,
                            ProductDetails = productDetails,
                            CustomerPhone = order.PhoneNo,
                            ShopAddress = shopAddress, // Set the ShopAddress
                            OfferStartTime = DateTime.Now // Store the offer start time
                        };

                        // Increment the nextOrderIndex for the next call
                        nextOrderIndex = (nextOrderIndex + 1) % ordersOnList.Count;

                        return offerViewModel;
                    }
                }
            }

            return null;
        }




        //not used

        [Authorize(Roles = "Rider")]
        private async Task AcceptedOrder(Order order, RiderModel rider)
        {
            decimal totalOrderAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);

            var delivery = new Delivery
            {
                OrderId = order.Id,
                RiderId = rider.Id,
                OrderCondition = OrderCondition.OrderTaken,
                PayableMoney = order.PaymentCondition == PaymentCondition.Paid ? 0 : totalOrderAmount,
                ProductDetails = order.OrderDetails.Select(od => od.Product.Name).FirstOrDefault(),
                CustomerAddress = order.Address,
                DelivyAddress = rider.Location,

            };

            _db.Deliveries.Add(delivery);
            await _db.SaveChangesAsync();

            order.IsOfferedToRider = true;
            order.OrderCondition = OrderCondition.OrderTaken;
            await _db.SaveChangesAsync();

            // Notify farmers for each product in the order
            foreach (var orderDetail in order.OrderDetails)
            {
                var product = orderDetail.Product;
                var farmerShop = _db.FarmerShop.Include(f => f.FarmerUser)
                                                .FirstOrDefault(f => f.Id == product.FarmerShopId);
                if (farmerShop != null)
                {
                    string farmerUserId = farmerShop.FarmerUserId;
                    _notificationService.AddNotification(farmerUserId, $"Order #{order.Id} has been accepted by a rider :  {delivery.RiderId}.for  Product: '{product.Name}'", product.Id);
                }
            }
        }




        [Authorize(Roles = "Rider")]
        public async Task<IActionResult> CreateDeliveryForAcceptedOrder()
        {
            // Get the current user's ID (rider's ID)
            var riderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the rider already exists in the database
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




        private string SerializeAddress(OrganicOption.Models.Address address)
        {
            return $"{address.Division}, {address.District}, {address.Thana}, {address.WardNo}, {address.StreetNo}, {address.House}";
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







        // Method to update FarmerShop revenue when products are released
        private async Task UpdateFarmerShopRevenue(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                decimal totalOrderPrice = order.OrderDetails.Sum(od => od.Price * od.Quantity);

                // Retrieve the FarmerShop associated with the order
                var farmerShop = order.OrderDetails.FirstOrDefault()?.Product?.FarmerShop;

                if (farmerShop != null)
                {
                    // Update FarmerShop's revenue
                    farmerShop.ShopRevenue += totalOrderPrice;

                    // Save changes to the database
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Handle the case where FarmerShop is not found
                }
            }
            else
            {
                // Handle the case where the order is not found
            }
        }







    }

}



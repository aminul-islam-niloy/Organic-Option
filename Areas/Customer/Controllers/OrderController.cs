using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.Payment;
using OnlineShop.Session;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using OrganicOption.Models;
using OrganicOption.Models.Rider_Section;
using OnlineShop.Service;

namespace OnlineShop.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class OrderController : Controller


    {

        private readonly StripeSettings _stripeSettings;

        public string SessionId { get; set; }

        private ApplicationDbContext _db;
        UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IOptions<StripeSettings> stripeSettings)
        {
            _db = db;
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;
        }


        //GET Checkout actioin method

        public IActionResult Checkout()
        {
            return View();
        }

        //POST Checkout action method

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order anOrder)
        {
            // Retrieve the user ID of the current authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Associate the order with the user ID
            anOrder.UserId = userId;
            // Set the order date to the current date and time
            anOrder.OrderDate = DateTime.Now;

            List<Products> products = HttpContext.Session.Get<List<Products>>("products");
            if (products != null)
            {
                foreach (var product in products)
                {
                    if (product.Discount > 0)
                    {
                        OrderDetails orderDetails = new OrderDetails
                        {
                            PorductId = product.Id,
                            Price = product.Price + product.DiscountPrice,  // Assuming product.Price represents the unit price
                            Quantity = product.QuantityInCart
                        };

                        anOrder.OrderDetails.Add(orderDetails);

                    }
                    else
                    {
                        OrderDetails orderDetails = new OrderDetails
                        {
                            PorductId = product.Id,
                            Price = product.Price,  // Assuming product.Price represents the unit price
                            Quantity = product.QuantityInCart
                        };

                        anOrder.OrderDetails.Add(orderDetails);

                    }




                }
            }

            // Set the order number
            anOrder.OrderNo = GetOrderNo();

            // Add the order to the database context
            _db.Orders.Add(anOrder);



            // Save changes to the database
            await _db.SaveChangesAsync();

            List<Products> Sesproducts = HttpContext.Session.Get<List<Products>>("products");
            if (Sesproducts != null)
            {
                foreach (var product in Sesproducts)
                {
                    // Update the inventory for each product
                    UpdateFarmerStore(product.Id, product.QuantityInCart, product.Price, anOrder.Id, product.FarmerShopId);
                }
            }

            // Clear the session data
            HttpContext.Session.Set("products", new List<Products>());

            // Redirect the user to the order confirmation page
            return RedirectToAction("PaymentPage", new { orderId = anOrder.Id });
        }


        //private void UpdateFarmerStore(int id, int quantitySold)
        //{
        //    Products product = _db.Products.FirstOrDefault(p => p.Id == id);

        //    if (product != null)
        //    {
        //        // Update the sold quantity and last sold date for the product
        //        product.SoldQuantity += quantitySold;
        //        product.LastSoldDate = DateTime.Now;

        //        // Retrieve the farmer store associated with the product
        //        FarmerShop farmerStore = _db.FarmerShop
        //            .Include(fs => fs.Inventory) // Include Inventory to access inventory items
        //            .FirstOrDefault(fs => fs.Id == product.FarmerShopId);

        //        // Update the sold quantity and last sold date for the product in the farmer's store
        //        if (farmerStore != null)
        //        {
        //            farmerStore.SoldQuantity += quantitySold;
        //            farmerStore.LastSoldDate = DateTime.Now;

        //            // Ensure that Inventory is not null
        //            if (farmerStore.Inventory == null)
        //            {
        //                farmerStore.Inventory = new List<InventoryItem>(); // Initialize if null
        //            }
        //            else
        //            {
        //                // Get all products in the inventory of the farmer's store
        //                var productsInInventory = farmerStore.Inventory.Select(item => item.Product);

        //            }

        //            // Mark products as sold in the inventory
        //            var inventoryItem = farmerStore.Inventory.FirstOrDefault(i => i.ProductId == id);
        //            if (inventoryItem != null)
        //            {
        //                inventoryItem.Quantity -= quantitySold;
        //                inventoryItem.LastSoldDate = DateTime.Now;
        //            }
        //        }

        //        // Save changes to the database
        //        _db.SaveChanges();
        //    }
        //}


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

                    // Include username
                    // Include email
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
                TotalPrice = order.OrderDetails.Sum(od => od.Quantity * od.Price),
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailsViewModel
                {
                    ProductId = od.PorductId,
                    ProductName = od.Product.Name,
                    ProductImage = od.Product.Image,

                    Quantity = od.Quantity,
                    Price = od.Price,
                    PaymentMethods = od.PaymentMethods,

                }).ToList()
            }).ToList();

            return View(viewModel);
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


        [AllowAnonymous]
        public async Task<IActionResult> DisplayOrdersOnList()
        {

            var ordersOnList = await _db.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist))
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            return View(ordersOnList);
        }





        [AllowAnonymous]
        public async Task<IActionResult> MyOffer()
        {
            var offer = await GetOfferForRider();

            //HttpContext.Session.Set("OfferData", offer);

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




        [AllowAnonymous]
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

                    if (order != null)
                    {
                        var productDetails = order.OrderDetails.Select(od => new ProductwithOrderViewModel
                        {

                            ProductName = od.Product.Name,
                            ProductImage = od.Product.Image, // Set the product image URL here
                            ShopName = od.Product.FarmerShop.ShopName,
                            ShopContact = od.Product.FarmerShop.ContractInfo,
                            Quantity = od.Quantity,

                            ShopAddress = od.Product.FarmerShop.ShopAddress,

                        }).ToList();

                        var offerViewModel = new RiderOfferViewModel
                        {
                            OrderId = order.Id,
                            CustomerAddress = order.Address,
                            DeliveryTime = EstimateDeliveryTime(order),
                            Revenue = CalculateEarnings(order),
                            ProductDetails = productDetails,
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
            await _db.SaveChangesAsync();
        }


        //private async Task CreateDeliveryForAcceptedOrder(RiderOfferViewModel offer, Order order)
        //{
        //    // Get the current user's ID (rider's ID)
        //    var riderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Check if the rider already exists in the database
        //    var existingRider = await _db.RiderModel.FirstOrDefaultAsync(rider => rider.RiderUserId == riderId);

        //    decimal totalOrderAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);

        //    var delivery = new Delivery
        //    {
        //        OrderId = offer.OrderId,
        //        RiderId = existingRider.Id, // Assign the rider's ID from the existing rider record
        //        OrderCondition = OrderCondition.OrderTaken,

        //        PayableMoney = order.PaymentCondition == PaymentCondition.Paid ? 0 : totalOrderAmount,
        //        ProductDetails = string.Join(", ", offer.ProductDetails.Select(product => product.ProductName)),
        //        DelivyAddress = offer.CustomerAddress,

        //        ShopAddress = offer.ShopAddress,

        //        ShopName = offer.ShopName,
        //        ShopContract = offer.ShopContract
        //    };

        //    _db.Deliveries.Add(delivery);
        //    await _db.SaveChangesAsync();

        //    //return View("DeliveryDetails", delivery);
        //}

        [AllowAnonymous]
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
        .ThenInclude(od => od.Product) // Include product information
            .ThenInclude(p => p.FarmerShop) // Include shop information
    .FirstOrDefaultAsync(o => o.Id == offer.OrderId);



                decimal totalOrderAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
                var delivery = new Delivery
                {
                    OrderId = offer.OrderId,
                    RiderId = existingRider.Id,
                    OrderCondition = OrderCondition.OrderTaken,
                    PayableMoney = order.PaymentCondition == PaymentCondition.Paid ? 0 : totalOrderAmount,
                    ProductDetails = string.Join(", ", offer.ProductDetails.Select(product => product.ProductName)),
                    CustomerAddress = offer.CustomerAddress, // Use offer.CustomerAddress for delivery address
                    DelivyAddress = offer.CustomerAddress,   // Use offer.CustomerAddress for delivery address
                    ShopAddress = offer.ShopAddress,
                    ShopName = offer.ShopName,
                    ShopContract = offer.ShopContract
                };


                try
                {
                    _db.Deliveries.Add(delivery);
                    await _db.SaveChangesAsync();

                    // Remove offer data from session after successful delivery creation
                    HttpContext.Session.Remove("OfferData");

                    // Return the view with the delivery details
                    return View("DeliveryDetails", delivery);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return View("Error");
                }
            }
            else
            {
                // Handle case when offer data is not available
                return View("Error");
            }
        }









        private async Task<RiderModel> FindAvailableRider()
        {
            var availableRider = await _db.RiderModel
                .FirstOrDefaultAsync(r => r.RiderStatus && !r.OnDeliaryByOffer);

            return availableRider;
        }




        private TimeSpan EstimateDeliveryTime(Order order)
        {
            // Example: Fixed average delivery time of 30 minutes
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





        //FarmerShop

        // Method to notify FarmerShop when Rider accepts an offer
        private async Task NotifyFarmerShop(RiderOfferViewModel offer, Order order)
        {


            if (order != null)
            {
                // Implement notification logic here to notify the FarmerShop
                // You can use email, SMS, or push notifications to inform the FarmerShop
                // Example: Send an email to the FarmerShop's contact email address
                //var emailService = new EmailService();
                //var message = $"Order {orderId} has been accepted by a rider. Please prepare the products for delivery.";
                //await emailService.SendEmailAsync(order.FarmerShop.ContactEmail, "Order Accepted Notification", message);
            }
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



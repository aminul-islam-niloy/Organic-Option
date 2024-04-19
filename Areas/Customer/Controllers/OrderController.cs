﻿using Microsoft.AspNetCore.Authorization;
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



        //[AllowAnonymous]
        //public async Task<IActionResult> MyOffer()
        //{
        //    var offer = await GetOfferForRider();

        //    if (offer != null)
        //    {
        //        // Check if the offer has expired
        //        if (offer.OfferStartTime.AddMinutes(1) < DateTime.Now)
        //        {
        //            // If expired, move to the next offer
        //            await MoveToNextOffer();
        //            // Wait for 1 second to ensure the next offer becomes visible
        //            await Task.Delay(1000);
        //            // Redirect to the same action after moving to the next offer
        //            return RedirectToAction(nameof(MyOffer));
        //        }

        //        var viewModel = new RiderOfferViewModel
        //        {
        //            OrderId = offer.OrderId,
        //            ProductDetails = offer.ProductDetails,
        //            CustomerAddress = offer.CustomerAddress,
        //            DeliveryTime = offer.DeliveryTime,
        //            Revenue = offer.Revenue,
        //            TimeRemaining = (offer.OfferStartTime.AddMinutes(1) - DateTime.Now)
        //        };

        //        return View(viewModel);
        //    }
        //    else
        //    {
        //        return Content("No offer available.");
        //    }
        //}



        [AllowAnonymous]
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
                    TimeRemaining = (offer.OfferStartTime.AddMinutes(1) - DateTime.Now)
                };

                return View(viewModel);
            }
            else
            {
                return Content("No offer available.");
            }
        }


        // Define a static variable to keep track of the index of the next order
        private static int nextOrderIndex = 0;

        //[AllowAnonymous]
        //private async Task<RiderOfferViewModel> GetOfferForRider()
        //{
        //    var availableRider = await FindAvailableRider();

        //    if (availableRider != null)
        //    {
        //        var ordersOnList = await _db.Orders
        //            .Include(o => o.OrderDetails)
        //            .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist) && !o.IsOfferedToRider)
        //            .OrderBy(o => o.Id) // Ensure order is fetched in ascending order of ID
        //            .ToListAsync();

        //        // Check if there are any orders available
        //        if (ordersOnList.Count > 0)
        //        {
        //            // Get the order based on the nextOrderIndex
        //            var order = ordersOnList.ElementAtOrDefault(nextOrderIndex);

        //            if (order != null)
        //            {
        //                var productDetails = order.OrderDetails
        //                    ?.Where(od => od.Product != null)
        //                    .Select(od => od.Product.Name)
        //                    .FirstOrDefault();

        //                var offerViewModel = new RiderOfferViewModel
        //                {
        //                    OrderId = order.Id,
        //                    ProductDetails = productDetails,
        //                    CustomerAddress = order.Address,
        //                    DeliveryTime = EstimateDeliveryTime(order),
        //                    Revenue = CalculateEarnings(order),
        //                    OfferStartTime = DateTime.Now // Store the offer start time
        //                };

        //                // Increment the nextOrderIndex for the next call
        //                nextOrderIndex = (nextOrderIndex + 1) % ordersOnList.Count;

        //                return offerViewModel;
        //            }
        //        }
        //    }

        //    return null;
        //}



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


        private async Task CreateDeliveryForAcceptedOrder(Order order, RiderModel rider)
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

            if (deliveryCharge < 50) {
                deliveryCharge = 70;
            }

            // Calculate earnings as 70% of the delivery charge
            decimal earnings = deliveryCharge * 0.70m;

            return earnings;



        }







        //public async Task<IActionResult> OfferOrdersToRider()
        //{
        //    var ordersOnList = await _db.Orders
        //             .Include(o => o.OrderDetails)
        //             .Where(o => o.OrderDetails.Any(od => od.OrderCondition == OrderCondition.Onlist))
        //             .OrderBy(o => o.OrderDate)
        //             .ToListAsync();


        //    // Logic to offer orders to available riders
        //    foreach (var order in ordersOnList)
        //    {
        //        // Offer the order to a rider (you need to implement this logic)
        //        bool isOfferAccepted = await OfferOrderToRider(order);

        //        // If the offer is accepted, mark the order as offered and exit the loop
        //        if (isOfferAccepted)
        //        {
        //            order.IsOfferedToRider = true;


        //            await _db.SaveChangesAsync();
        //            break;
        //        }
        //    }

        //    return RedirectToAction(nameof(DisplayOrdersOnList));
        //}

        //private async Task<bool> OfferOrderToRider(Order order)
        //{
        //    var availableRider = await FindAvailableRider();

        //    if (availableRider != null)


        //    {

        //        decimal totalOrderAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
        //        // Create a new delivery entry to associate the order with the rider
        //        var delivery = new Delivery
        //        {
        //            OrderId = order.Id,
        //            RiderId = availableRider.Id,
        //            OrderCondition = OrderCondition.OrderTaken,
        //            ProductDetails = order.OrderDetails.Select(od => od.Product.Name).FirstOrDefault(),
        //            CustomerAddress = order.Address,
        //            DelivyAddress = availableRider.Location,
        //            FarmerShopId = order.FarmerShopId,
        //            PayableMoney = order.PaymentCondition == PaymentCondition.Paid ? 0 : totalOrderAmount,


        //        };

        //        // Add the delivery to the database
        //        _db.Deliveries.Add(delivery);
        //        await _db.SaveChangesAsync();

        //        // Mark the order as offered to a rider
        //        order.IsOfferedToRider = true;
        //        await _db.SaveChangesAsync();

        //        return true; // Offer accepted by the rider
        //    }
        //    else
        //    {
        //        return false; // No available rider found
        //    }
        //}


        private async Task<RiderModel> FindAvailableRider()
        {
            var availableRider = await _db.RiderModel
                .FirstOrDefaultAsync(r => r.RiderStatus && !r.OnDeliaryByOffer);

            return availableRider;
        }


        //public async Task<IActionResult> RiderAcceptOrder(int orderId)
        //{
        //    var order = await _db.Orders
        //        .Include(o => o.OrderDetails)
        //        .FirstOrDefaultAsync(o => o.Id == orderId && o.OrderCondition == OrderCondition.Onlist && o.IsOfferedToRider);

        //    if (order != null)
        //    {
        //        // Calculate earnings and time for the rider to deliver the order
        //        decimal earnings = CalculateEarnings(order);
        //        TimeSpan deliveryTime = EstimateDeliveryTime(order);

        //        // Proceed with preparing the products for release to the rider
        //        await PrepareProductsForRelease(order);

        //        // Return view showing earnings and delivery time to the rider
        //        return View("RiderAcceptOrder", new RiderAcceptOrderViewModel { Order = order, Earnings = earnings, DeliveryTime = deliveryTime });
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}

        //private decimal CalculateEarnings(Order order)
        //{
        //    // Logic to calculate earnings based on order details
        //    // You need to implement this logic according to your system's requirements
        //}

        //private TimeSpan EstimateDeliveryTime(Order order)
        //{
        //    // Logic to estimate delivery time based on order details and rider's location
        //    // You need to implement this logic according to your system's requirements
        //}

        //private async Task PrepareProductsForRelease(Order order)
        //{
        //    // Logic to prepare products for release to the rider
        //    // You need to implement this logic according to your system's requirements
        //}







    }

}



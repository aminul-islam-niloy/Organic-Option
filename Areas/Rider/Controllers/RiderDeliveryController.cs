using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OrganicOption.Models.Rider_Section;
using Stripe.Climate;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganicOption.Areas.Rider.Controllers
{
    [Area("Rider")]
    [Authorize(Roles = "Rider")]
    public class RiderDeliveryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public RiderDeliveryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IActionResult> Index()
        {

            var riderUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rider = await _dbContext.RiderModel.FirstOrDefaultAsync(r => r.RiderUserId == riderUserId);

            if (rider == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var deliveries = await _dbContext.Deliveries
                .Where(d => d.RiderId == rider.Id)
                .ToListAsync();

            return View(deliveries);
        }


        public async Task<IActionResult> Details(int id)
        {

            var riderUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rider = await _dbContext.RiderModel.FirstOrDefaultAsync(r => r.RiderUserId == riderUserId);

            if (rider == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }


            var delivery = await _dbContext.Deliveries
                .FirstOrDefaultAsync(d => d.Id == id && d.RiderId == rider.Id);

            if (delivery == null)
            {

                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var order = await _dbContext.Orders
                    .Include(o => o.CustomerAddress).Include(o => o.OrderDetails)
                     .ThenInclude(od => od.Product)
                     .FirstOrDefaultAsync(o => o.Id == delivery.OrderId);
            ViewBag.CustomerAddress = order.CustomerAddress;
           
            var farmerShopId = order.OrderDetails.FirstOrDefault()?.Product.FarmerShopId;

            var farmerShop = await _dbContext.FarmerShop.Include(a => a.ShopAddress)
            .FirstOrDefaultAsync(fs => fs.Id == farmerShopId);

            ViewBag.ShopAddress = farmerShop.ShopAddress;

            return View(delivery);
        }



        // Action to display currently running deliveries
        public async Task<IActionResult> RunningDeliveries()
        {
            var riderId = GetCurrentRiderId();
            if (riderId == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var deliveries = await _dbContext.Deliveries
                .Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.OnDelivary )
                .ToListAsync();

            return View(deliveries);
        }

        //  Action to display delivered orders grouped by daily, weekly, and monthly
        //https://localhost:44343/Rider/RiderDelivery/DeliveredOrders/?period=weekly
        public async Task<IActionResult> DeliveredOrders(string period)
        {
            var riderId = GetCurrentRiderId();
            if (riderId == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (period == null)
            {
                return RedirectToAction("Index", "RiderDelivery", new { area = "Rider" });
            }

            IQueryable<Delivery> query = _dbContext.Deliveries.Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.Delivered);

          
            switch (period.ToLower())
            {
                case "daily":
                    var today = DateTime.Now.Date;
                    query = query.Where(d => d.OrderDeliveredDate.Date == today);
                    break;
                case "weekly":
                    var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                    query = query.Where(d => d.OrderDeliveredDate >= startOfWeek && d.OrderDeliveredDate < startOfWeek.AddDays(7));
                    break;
                case "monthly":
                    query = query.Where(d => d.OrderDeliveredDate.Month == DateTime.Now.Month && d.OrderDeliveredDate.Year == DateTime.Now.Year);
                    break;
                default:
                    return BadRequest("Invalid period specified.");
            }

            var deliveries = await query.ToListAsync();

            ViewBag.Period = period;
            return View(deliveries);
        }

        // Helper method to get the current rider's ID
        private int? GetCurrentRiderId()
        {
            var riderUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rider = _dbContext.RiderModel.FirstOrDefault(r => r.RiderUserId == riderUserId);
            return rider?.Id;
        }

        

        // Action to calculate and display rider revenue and due amounts
        public async Task<IActionResult> RiderRevenue()
        {
            var riderId = GetCurrentRiderId();
            if (riderId == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var deliveries = await _dbContext.Deliveries
                .Where(d => d.RiderId == riderId)
                .ToListAsync();

            decimal totalRevenue = deliveries.Sum(d => d.PayableMoney);
            decimal totalDue = deliveries.Where(d => d.OrderCondition == OrderCondition.Delivered).Sum(d => d.PayableMoney);

            var rider = await _dbContext.RiderModel.FirstOrDefaultAsync(r => r.Id == riderId);
            if (rider != null)
            {
                rider.Revenue = totalRevenue;
                rider.RiderDue = totalDue;
                await _dbContext.SaveChangesAsync();
            }

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalDue = totalDue;

            return View();
        }


        //[HttpPost]
        public async Task<IActionResult> ConfirmDelivery(int deliveryId)
        {
            var riderUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rider = await _dbContext.RiderModel.FirstOrDefaultAsync(r => r.RiderUserId == riderUserId);

            if (rider == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            // Find the delivery and related order by deliveryId and riderId
            var delivery = await _dbContext.Deliveries
                .FirstOrDefaultAsync(d => d.Id == deliveryId && d.RiderId == rider.Id);

            if (delivery == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == delivery.OrderId);
            if (order == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            // Update revenue and due
            rider.Revenue += order.DelivaryCharge;
            rider.RiderDue += delivery.PayableMoney;

            // Update order condition
            order.OrderCondition = OrderCondition.Delivered;
            delivery.OrderCondition= OrderCondition.Delivered;

         
            rider.OnDeliaryByOffer = false;
            delivery.OrderDeliveredDate = DateTime.Now; 

            // Save changes
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("RunningDeliveries");
        }

    }

}

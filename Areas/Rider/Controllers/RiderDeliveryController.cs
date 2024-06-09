using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OrganicOption.Models.Rider_Section;
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

                return NotFound();
            }

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
                .Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.OnDelivary)
                .ToListAsync();

            return View(deliveries);
        }

        // Action to display delivered orders grouped by daily, weekly, and monthly
        //public async Task<IActionResult> DeliveredOrders(string period)
        //{
        //    var riderId = GetCurrentRiderId();
        //    if (riderId == null)
        //    {
        //        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //    }

        //    IQueryable<Delivery> query = _dbContext.Deliveries.Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.Delivered);

        //    DateTime now = DateTime.Now;
        //    switch (period.ToLower())
        //    {
        //        case "daily":
        //            query = query.Where(d => d.OrderDeliveredDate.Date == now.Date);
        //            break;
        //        case "weekly":
        //            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        //            query = query.Where(d => d.OrderDeliveredDate >= startOfWeek && d.OrderDeliveredDate < startOfWeek.AddDays(7));
        //            break;
        //        case "monthly":
        //            query = query.Where(d => d.OrderDeliveredDate.Month == now.Month && d.OrderDeliveredDate.Year == now.Year);
        //            break;
        //        default:
        //            return BadRequest("Invalid period specified.");
        //    }

        //    var deliveries = await query.ToListAsync();

        //    ViewBag.Period = period;
        //    return View(deliveries);
        //}

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
    }

}

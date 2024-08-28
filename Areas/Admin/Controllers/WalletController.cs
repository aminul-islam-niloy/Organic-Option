using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using System.Security.Claims;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using OnlineShop.Models;
using OrganicOption.Models.Rider_Section;
using OrganicOption.Models;
using OrganicOption.Service;
using Stripe.Climate;

namespace OrganicOption.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _db;

        UserManager<IdentityUser> _userManager;
        private readonly NotificationService _notificationService;

        public WalletController(ApplicationDbContext db, UserManager<IdentityUser> userManager, NotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _notificationService = notificationService;
        }
        public  IActionResult AdminViewWithdrawals()
        {
            var withdrawalRequests = _db.withdrawalHistories
                     .Where(w => !w.IsConfirmed)
                .ToList();

          
            return View(withdrawalRequests);
        }

        [HttpPost]
        public IActionResult ApproveWithdraw(int id)
        {
            var request = _db.withdrawalHistories.Find(id);
            if (request == null) return NotFound();

            request.IsApproved = true;
            _db.SaveChanges();

            return RedirectToAction("AdminViewWithdrawals");
        }

        [HttpPost]
        public IActionResult ConfirmWithdraw(int id)
        {
            var request = _db.withdrawalHistories.Find(id);
            if (request == null || !request.IsApproved) return BadRequest();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            request.IsConfirmed = true;
            request.ConfirmDate = DateTime.Now;
            request.AdminId = adminId;

            if (request.UserType == "Rider")
            {
                var rider = _db.RiderModel.SingleOrDefault(r => r.RiderUserId == request.UserId);
                if (rider == null || (rider.Revenue + rider.RiderDue) < request.Amount)
                {
                    TempData["Error"] = "Insufficient funds or rider not found.";
                    return RedirectToAction("AdminViewWithdrawals");
                }

              
                if (rider.Revenue >= request.Amount)
                {
                    rider.Revenue -= request.Amount;
                }
                else
                {
                    var remainingAmount = request.Amount - rider.Revenue;
                    rider.Revenue = 0;
                    rider.RiderDue -= remainingAmount;
                }
                var RiderUserId = rider.RiderUserId;
                
                _notificationService.AddNotification(RiderUserId, $"Your Withdraw #{request.Id} Request is accepted. You can track it", request.Id);
            }
            else if (request.UserType == "Farmer")
            {
                var farmer = _db.FarmerShop.SingleOrDefault(f => f.FarmerUserId == request.UserId);
                if (farmer == null || farmer.ShopRevenue < request.Amount)
                {
                    TempData["Error"] = "Insufficient funds or farmer not found.";
                    return RedirectToAction("AdminViewWithdrawals");
                }

                farmer.ShopRevenue -= request.Amount;

                var FarmerUserId = farmer.FarmerUserId;

                _notificationService.AddNotification(FarmerUserId, $"Your Withdraw #{request.Id} Request is accepted. You can track it", request.Id);
            }

          

            _db.SaveChanges();
            _db.SaveChangesAsync();

            return RedirectToAction("AdminViewWithdrawals");
        }


        public IActionResult AllWithdrawals()
        {
            // Calculate total confirmed revenue
            var totalRevenue = _db.withdrawalHistories
                .Where(w => w.IsConfirmed)
                .Sum(w => w.Amount);

            // Group confirmed withdrawals by date (in the current month)
            var confirmedWithdrawals = _db.withdrawalHistories
                .Where(w => w.IsConfirmed && w.ConfirmDate.HasValue && w.ConfirmDate.Value.Month == DateTime.Now.Month)
                .OrderBy(w => w.ConfirmDate)
                .ToList();

         
            ViewBag.TotalRevenue = totalRevenue;
            return View(confirmedWithdrawals);
        }
    }
}

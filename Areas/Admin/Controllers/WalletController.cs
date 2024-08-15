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

namespace OrganicOption.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _db;

        UserManager<IdentityUser> _userManager;
      

        public WalletController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public  async Task<IActionResult> AdminViewWithdrawals()
        {
            var withdrawalRequests = _db.withdrawalHistories
                     .Where(w => !w.IsConfirmed)
                .ToList();



      
            //foreach (var request in withdrawalRequests)
            //{
            //    // Ensure that _userManager is of type UserManager<ApplicationUser>
            //    request.User = await _userManager.FindByIdAsync(request.UserId) as ApplicationUser;
            //}
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

                // Deduct from Revenue first, then from Due if necessary
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
            }

          

            _db.SaveChanges();
            _db.SaveChangesAsync();

            return RedirectToAction("AdminViewWithdrawals");
        }
    }
}

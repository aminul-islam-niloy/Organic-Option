using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OrganicOption.Models.Rider_Section;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganicOption.Areas.User.Controllers
{
    [Area("User")]
    public class DeshboardController : Controller
    {

        UserManager<IdentityUser> _userManager;
        ApplicationDbContext _db;
        private readonly RiderRepository _riderRepository;
        public DeshboardController(UserManager<IdentityUser> userManager, ApplicationDbContext db, RiderRepository riderRepository)
        {
            _userManager = userManager;
            _db = db;
            _riderRepository = riderRepository;
        }


        public IActionResult Index()
        {
            return View();
        }



        public async Task<IActionResult> MyAccount(string id)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == id);
            if (user == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }


            if (currentUser.Id != user.Id)
            {
                return Forbid();
            }

            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id != id)
            {

                return Forbid();
            }

            var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == id);
            if (user == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            return View(user);
        }



        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser user, IFormFile profilePicture)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id != user.Id)
            {

                return Forbid();
            }


            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);
            if (userInfo == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            userInfo.FirstName = user.FirstName;
            userInfo.LastName = user.LastName;
            userInfo.PhoneNumber = user.PhoneNumber;
            userInfo.Address = user.Address;
            userInfo.DateOfBirth = user.DateOfBirth;

            // Handle profile picture upload
            if (profilePicture != null && profilePicture.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profilePicture.CopyToAsync(memoryStream);
                    userInfo.ProfilePicture = memoryStream.ToArray();
                }
            }


            var result = await _userManager.UpdateAsync(userInfo);
            if (result.Succeeded)
            {
                TempData["save"] = "User has been updated successfully";
                // Redirect to the dashboard with userId
                return RedirectToAction("MyAccount", new { id = user.Id });
            }
            return View(userInfo);

        }



        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> UpdateLocation(double latitude, double longitude)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);


            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }


            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);
            if (userInfo == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            userInfo.Latitude = latitude;
            userInfo.Longitude = longitude;

            await _userManager.UpdateAsync(userInfo);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id != id)
            {

                return Forbid();
            }

            var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == id);
            if (user == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ApplicationUser user)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id != user.Id)
            {

                return Forbid();
            }

            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);
            if (userInfo == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });

            }
            _db.ApplicationUser.Remove(userInfo);
            int rowAffected = _db.SaveChanges();
            if (rowAffected > 0)
            {

                return RedirectToAction(nameof(System.Index));
            }
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public async Task<IActionResult> Lockout(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == id);
            if (user == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Lockout(ApplicationUser user)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var userInfo = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == user.Id);
            if (userInfo == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            userInfo.LockoutEnd = DateTime.Now.AddDays(1);
            await _userManager.UpdateAsync(userInfo);



            var result = await _userManager.UpdateAsync(userInfo);
            if (result.Succeeded)
            {
                TempData["lockout"] = "User has been locked out successfully";
                // Redirect to the dashboard with userId
                return RedirectToAction("Deshboard", new { id = user.Id });
            }

            return RedirectToAction("Deshboard", new { id = user.Id });
        }

        public async Task<IActionResult> Active(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == id);
            if (user == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }



            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Active(ApplicationUser user)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            var userInfo = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == user.Id);
            if (userInfo == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }

            userInfo.LockoutEnd = null;
            await _userManager.UpdateAsync(userInfo);

            var result = await _userManager.UpdateAsync(userInfo);
            if (result.Succeeded)
            {
                TempData["lockout"] = "User has been Active successfully";
                // Redirect to the dashboard with userId
                return RedirectToAction("Deshboard", new { id = user.Id });
            }


            return RedirectToAction(nameof(System.Index));
        }



        public async Task<IActionResult> MyDashboard()
        {
            //var currentUser = await _userManager.GetUserAsync(User);
            //if (currentUser == null)
            //{
            //    return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            //}

            //var user = await _db.ApplicationUser.FirstOrDefaultAsync(c => c.Id == currentUser.Id);
            //if (user == null)
            //{
            //    return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            //}

            //if (currentUser.Id != user.Id)
            //{
            //    return Forbid();
            //}

            if (User.IsInRole("Rider"))
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var rider = _db.RiderModel.Include(r => r.RiderAddress).SingleOrDefault(r => r.RiderUserId == user);
                var monthlyRevenue = _riderRepository.GetMonthlyRevenue(rider.Id);
                var totalRevenue = _riderRepository.GetTotalRevenue(rider.Id);
                var performance = _riderRepository.GetRiderPerformance(rider.Id);

                var currentMonth = DateTime.Now.ToString("MMMM");

                var currentMonthPerformance = performance.FirstOrDefault(p => p.MonthName == currentMonth);
                var currentMonthTotalDeliveries = currentMonthPerformance?.CompletedDeliveries ?? 0;
                var currentMonthTotalRevenue = currentMonthPerformance?.EarnedRevenue ?? 0;

                ViewBag.CurrentMonthTotalDeliveries = currentMonthTotalDeliveries;
                ViewBag.CurrentMonthTotalRevenue = currentMonthTotalRevenue.ToString();

                ViewBag.TotalRevenue = rider.Revenue.ToString();

                //var riderDashboardViewModel = new RiderDashboardViewModel
                //{
                //    MonthlyRevenue = monthlyRevenue,
                //    TotalRevenue = totalRevenue,
                //    Performance = performance
                //};

                ViewBag.MonthlyRevenue = monthlyRevenue;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.Performance = performance;

                return View();
            }

            if (User.IsInRole("Rider"))
            {

                var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var farmer = _db.FarmerShop.SingleOrDefault(f => f.FarmerUserId == farmerId);

                var farmerShop = await _db.FarmerShop
            .Include(f => f.ShopAddress)
            .FirstOrDefaultAsync(m => m.Id == farmer.Id);



            }




                return View(); 
        }







    }
}

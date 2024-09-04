using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OrganicOption.Models;
using OrganicOption.Models.Farmer_Section;
using OrganicOption.Models.Rider_Section;
using System;
using System.Collections.Generic;
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

              

                ViewBag.MonthlyRevenue = monthlyRevenue;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.Performance = performance;

                return View();
            }

            if (User.IsInRole("Farmer"))
            {

                var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var farmer = _db.FarmerShop.SingleOrDefault(f => f.FarmerUserId == farmerId);

                var farmerShop = await _db.FarmerShop
                     .Include(f => f.ShopAddress)
                     .FirstOrDefaultAsync(m => m.Id == farmer.Id);

                ViewBag.TotalRevenue = farmerShop.ShopRevenue;
                ViewBag.ShopId= farmerShop.Id;

                var monthlyRevenue = GetMonthlyRevenue(farmerShop.Id);

                // Get Performance Data
                var performanceData = GetFarmerShopPerformance(farmerShop.Id);

                // Pass the data to the view
                ViewBag.MonthlyRevenue = monthlyRevenue;
                ViewBag.Performance = performanceData;

                var currentMonthRevenue = GetCurrentMonthRevenue(farmerShop.Id);
                ViewBag.CurrentMonthRevenue = currentMonthRevenue;


                return View();
            }




          return View(); 
        }

        //FarmerShop Performance and Revenue

        public Dictionary<int, decimal> GetMonthlyRevenue(int farmerShopId)
        {
            var inventoryItems = _db.InventoryItem
                .Where(ii => ii.FarmerShopId == farmerShopId)
                .Join(_db.Deliveries,
                    ii => ii.OrderId,
                    d => d.OrderId,
                    (ii, d) => new
                    {
                        d.OrderAcceptTime,
                        ii.Price,
                        d.OrderCondition
                    })
                .Where(x => x.OrderCondition == OrderCondition.Delivered && x.OrderAcceptTime != null)
                .ToList();

     

            // Group by month and sum up the prices
            var monthlyRevenue = inventoryItems
                .GroupBy(ii => ii.OrderAcceptTime.Month)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(ii => ii.Price)
                );

            return monthlyRevenue;
        }



        public List<FarmerShopPerformance> GetFarmerShopPerformance(int farmerShopId)
        {
            var performanceData = from ii in _db.InventoryItem
                                  join d in _db.Deliveries on ii.OrderId equals d.OrderId
                                  where ii.FarmerShopId == farmerShopId && d.OrderCondition == OrderCondition.Delivered
                                  group ii by new { d.OrderAcceptTime.Year, d.OrderAcceptTime.Month } into g
                                  select new FarmerShopPerformance
                                  {
                                      Month = g.Key.Month,
                                      MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                                      SoldProducts = g.Count(),
                                      EarnedRevenue = g.Sum(ii => ii.Price)
                                  };

         
            return performanceData
                .OrderBy(p => p.Month)
                .ToList();
        }




        public decimal GetCurrentMonthRevenue(int farmerShopId)
        {
            var monthlyRevenue = GetMonthlyRevenue(farmerShopId);
            var currentMonth = DateTime.Now.Month;

            var currentMonthRevenue = monthlyRevenue.ContainsKey(currentMonth)
                ? monthlyRevenue[currentMonth]
                : 0;

            return currentMonthRevenue;
        }











    }
}

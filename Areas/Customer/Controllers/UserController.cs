using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserController : Controller
    {
        UserManager<IdentityUser> _userManager;
        ApplicationDbContext _db;
        public UserController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            //var dd = _userManager.GetUserId(HttpContext.User);
            return View(await _db.ApplicationUser.ToListAsync());
        }


        public IActionResult Create()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                // Check if the username already exists
                var existingUser = await _userManager.FindByNameAsync(user.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Username already exists.");
                    return View(user);
                }

                // Proceed with user creation if the username is unique
                var result = await _userManager.CreateAsync(user, user.PasswordHash);

                if (result.Succeeded)
                {
                    TempData["save"] = "User has been created successfully";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(user);
        }


        public async Task<IActionResult> Details(string id)
        {
            var user =  _db.ApplicationUser.FirstOrDefault(c => c.Id == id);
            if (user == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            return View(user);


        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = _db.ApplicationUser.FirstOrDefault(c => c.Id == id);
            if (user == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ApplicationUser user)
        {
            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);
            if (userInfo == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });

            }
            _db.ApplicationUser.Remove(userInfo);
            int rowAffected = _db.SaveChanges();
            if (rowAffected > 0)
            {
                TempData["save"] = "User has been delete successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(userInfo);
        }


        public async Task<IActionResult> Locout(string id)
        {
            if (id == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            var user = _db.ApplicationUser.FirstOrDefault(c => c.Id == id);
            if (user == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Locout(ApplicationUser user)
        {
            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);
            if (userInfo == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });

            }
            userInfo.LockoutEnd = DateTime.Now.AddDays(3);
            int rowAffected = _db.SaveChanges();
            if (rowAffected > 0)
            {
                TempData["locout"] = "User has been lockout successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(userInfo);
        }

        public async Task<IActionResult> Active(string id)
        {
            var user = _db.ApplicationUser.FirstOrDefault(c => c.Id == id);
            if (user == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Active(ApplicationUser user)
        {
            var userInfo = _db.ApplicationUser.FirstOrDefault(c => c.Id == user.Id);
            if (userInfo == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });

            }
            //userInfo.LockoutEnd = DateTime.Now.AddDays(-1);
            userInfo.LockoutEnd = null;
            int rowAffected = _db.SaveChanges();
            if (rowAffected > 0)
            {
                TempData["locout"] = "User has been active successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(userInfo);
        }



    }
}

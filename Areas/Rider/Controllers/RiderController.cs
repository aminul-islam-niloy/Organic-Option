using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineShop.Data;
using OrganicOption.Models;
using OrganicOption.Models.Rider_Section;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicOption.Areas.Rider.Controllers
{

    [Area("Rider")]
    [Authorize(Roles = "Rider")]
    public class RiderController : Controller

    {
        private readonly ApplicationDbContext _context;
        UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;

        public RiderController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cache = memoryCache;
        }
        public async Task<IActionResult> Riders()
        {

            // Retrieve all shops in FarmerShop
            var riders = await _context.RiderModel.Include(f => f.RiderAddress).ToListAsync();

            return View(riders);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve all riders for the current user
            var riders = await _context.RiderModel
                .Include(r => r.RiderAddress)
                .Where(r => r.RiderUserId == currentUser.Id).ToListAsync();

            if (riders == null || !riders.Any())
            {
                return RedirectToAction(nameof(CreateRider)); // Current user does not have any riders
            }

            return View(riders);
        }


        public ActionResult CreateRider()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRider(RiderModel rider)
        {
         
            var currentUser = await _userManager.GetUserAsync(User);

            var existingRider = await _context.RiderModel.FirstOrDefaultAsync(rider => rider.RiderUserId == currentUser.Id);
            if (existingRider != null)
            {
               
                return RedirectToAction(nameof(RiderExist)); 
            }

           rider.RiderUserId = currentUser.Id;
           //rider.RiderUserId= User.Identity.Name;

            _context.RiderModel.Add(rider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> EditRider()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }

            var existingRider = await _context.RiderModel
                   .Include(r => r.RiderAddress)
                .FirstOrDefaultAsync(r => r.RiderUserId == currentUser.Id);

            if (existingRider == null)
            {
                return NotFound();
            }

            return View(existingRider);
        }

        [HttpPost]
        public async Task<IActionResult> EditRider(RiderModel rider)
        {
           

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }

            var existingRider = await _context.RiderModel.FirstOrDefaultAsync(r => r.RiderUserId == currentUser.Id);

            if (existingRider == null)
            {
                return NotFound();
            }

            existingRider.Name = rider.Name;
            existingRider.Age = rider.Age;
            existingRider.DrivingLicense = rider.DrivingLicense;
            existingRider.NID = rider.NID;
            existingRider.PhoneNumber = rider.PhoneNumber;
            existingRider.RiderAddress = rider.RiderAddress; 

            _context.RiderModel.Update(existingRider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> EditShift()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }

            var existingRider = await _context.RiderModel
                .Include(r => r.RiderAddress)
                .FirstOrDefaultAsync(r => r.RiderUserId == currentUser.Id);

            if (existingRider == null)
            {
                return NotFound();
            }

            // Check if the rider is currently in a shift
            var currentShift = await _context.Shifts.FirstOrDefaultAsync(s => s.RiderId == existingRider.Id && s.RiderStatus);

            ViewBag.IsInShift = currentShift != null;

            return View(existingRider);
        }


        [HttpPost]
        public async Task<IActionResult> EditShift(RiderModel rider)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }

            var existingRider = await _context.RiderModel.FirstOrDefaultAsync(r => r.RiderUserId == currentUser.Id);

            if (existingRider == null)
            {
                return NotFound();
            }

            // Start a new shift
            var newShift = new Shift
            {
                StartTime = DateTime.Now,
                RiderId = existingRider.Id,
                RiderStatus = true
            };

            // Update rider details
            existingRider.RiderStatus = true;
            existingRider.BagType = rider.BagType;
            existingRider.VehicleType = rider.VehicleType;

            // Save changes to shift and rider
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Shifts.Add(newShift);
                    await _context.SaveChangesAsync();

                    _context.RiderModel.Update(existingRider);
                    await _context.SaveChangesAsync();

          
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                  
                    await transaction.RollbackAsync();
                    
                    ModelState.AddModelError(string.Empty, "Failed to start shift. Please try again.");
                    return View(existingRider); // Return to the view to display the error
                }
            }

            return RedirectToAction(nameof(Index));
        }

    

    [HttpGet]
        public async Task<IActionResult> UpdateShift()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }

            var existingRider = await _context.RiderModel
                .Include(r => r.RiderAddress)
                .FirstOrDefaultAsync(r => r.RiderUserId == currentUser.Id);

            if (existingRider == null)
            {
                return NotFound();
            }

            // Check if the rider is currently in a shift
            var currentShift = await _context.Shifts.FirstOrDefaultAsync(s => s.RiderId == existingRider.Id && s.RiderStatus);

            ViewBag.IsInShift = currentShift != null;

            return View(existingRider);
        }

    


        [HttpPost]
        public async Task<IActionResult> UpdateShift(RiderModel rider)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound();
            }

            var existingRider = await _context.RiderModel.FirstOrDefaultAsync(r => r.RiderUserId == currentUser.Id);

            if (existingRider == null)
            {
                return NotFound();
            }

            // Start a new shift
            var endShift = new Shift
            {
                EndTime = DateTime.Now,
                RiderId = existingRider.Id,
                RiderStatus = false
            };

            // Update rider details
            existingRider.RiderStatus = false;
            existingRider.BagType = rider.BagType;
            existingRider.VehicleType = rider.VehicleType;

            // Save changes to shift and rider
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Shifts.Add(endShift);
                    await _context.SaveChangesAsync();

                    _context.RiderModel.Update(existingRider);
                    await _context.SaveChangesAsync();

        
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    // Rollback the transaction if there is an exception
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(string.Empty, "Failed to start shift. Please try again.");
                    return View(existingRider); 
                }
            }

            return RedirectToAction(nameof(Index));
        }



        public IActionResult RiderExist()
        {
            return View();
        }



    }

    



    }

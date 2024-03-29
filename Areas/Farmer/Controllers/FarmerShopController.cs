﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineShop.Data;
using OnlineShop.Models;
using OrganicOption.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicOption.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    [Authorize(Roles = "Customer")]
    public class FarmerShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;
        public FarmerShopController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cache = memoryCache;
        }


 

        public async Task<IActionResult> Index()
        {
            
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Retrieve the shop for the current user
            var farmerShop = await _context.FarmerShop
                .Include(fs => fs.Products)
                .FirstOrDefaultAsync(fs => fs.FarmerUserId == currentUser.Id);

            if (farmerShop == null)
            {
                return RedirectToAction(nameof(CreateShop)); // Current user does not have a shop
            }

            return View(farmerShop);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FarmerShop farmerShop, IFormFile coverPhoto)
        {
            //if (ModelState.IsValid)
            //{
          
            // Handle the cover photo upload
            if (coverPhoto != null && coverPhoto.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await coverPhoto.CopyToAsync(stream);
                        farmerShop.CoverPhoto = stream.ToArray();
                    }
                }
                // Set the current user as the farmer for this shop
                var currentUser = await _userManager.GetUserAsync(User);

            var existingShop = await _context.FarmerShop.FirstOrDefaultAsync(shop => shop.FarmerUserId == currentUser.Id);
            if (existingShop != null)
            {
                // Redirect the user or display a message indicating they cannot create another shop
                return RedirectToAction(nameof(ShopExist)); // Example: Redirect to an error page indicating that the user already has a shop
            }

            farmerShop.FarmerUser = (OnlineShop.Models.ApplicationUser)currentUser;
                //farmerShop.FarmerUserId = User.Identity.Name;

                // Add the farmer shop to the database
                _context.FarmerShop.Add(farmerShop);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            //}
            //return View(farmerShop);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerShop = _context.FarmerShop.Find(id);
            if (farmerShop == null)
            {
                return NotFound();
            }

            return View(farmerShop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FarmerShop farmerShop, IFormFile coverPhoto)
        {
            if (id != farmerShop.Id)
            {
                return NotFound();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            farmerShop.FarmerUser = (OnlineShop.Models.ApplicationUser)currentUser;

            try
                {
                if (coverPhoto != null && coverPhoto.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await coverPhoto.CopyToAsync(stream);
                        farmerShop.CoverPhoto = stream.ToArray();
                    }
                }

                _context.Update(farmerShop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FarmerShopExists(farmerShop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
          
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerShop = await _context.FarmerShop
                .FirstOrDefaultAsync(m => m.Id == id);
            if (farmerShop == null)
            {
                return NotFound();
            }

            return View(farmerShop);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var farmerShop = await _context.FarmerShop.FindAsync(id);
            _context.FarmerShop.Remove(farmerShop);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FarmerShopExists(int id)
        {
            return _context.FarmerShop.Any(e => e.Id == id);
        }

        public IActionResult ShopExist()
        {
            return View();
        }

        public IActionResult CreateShop()
        {
            return View();
        }

        public  IActionResult AddProduct()
        {
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");


            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddProduct(Products product, List<IFormFile> ImagesSmall)
        {
            if (ModelState.IsValid)
            {
               

                var currentUser = await _userManager.GetUserAsync(User);

                // Retrieve the FarmerShopId associated with the current user
                var currentShop = await _context.FarmerShop.FirstOrDefaultAsync(shop => shop.FarmerUserId == currentUser.Id);
                if (currentShop != null)
                {
                    // FarmerShop found, set its ID for the product being created
                    product.FarmerShopId = currentShop.Id;
                }

                product.CreationTime = DateTime.Now;
                //product.ExpirationTime = product.ExpirationTime.Date + product.ExpirationTime.TimeOfDay;

             //   DateTime combinedExpirationTime = ExpirationTime.Date + ExpirationTime.TimeOfDay;

                // Handle the ExpirationTime
                if (product.ExpirationTime < DateTime.Now)
                {
                    ModelState.AddModelError(nameof(product.ExpirationTime), "Expiration time cannot be in the past.");
                    // Return the view with the validation error
                    return View(product);
                } 

                //Product add Section

                var searchProduct = _context.Products.FirstOrDefault(c => c.Name == product.Name);
                if (searchProduct != null)
                {
                    ViewBag.message = "This product already exists";
                    ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
                    ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");
                    return View(product);
                }

                if (ImagesSmall != null && ImagesSmall.Count > 0)
                {
                    product.ImagesSmall = new List<ProductImage>(); // Initialize the collection

                    foreach (var image in ImagesSmall)
                    {
                        try
                        {
                            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images", Path.GetFileName(image.FileName));

                            // Ensure the directory exists
                            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                            using (var fileStream = new FileStream(imagePath, FileMode.Create))
                            {
                                await image.CopyToAsync(fileStream);
                            }

                            // Add the image path to the product's images collection
                            product.ImagesSmall.Add(new ProductImage { ImagePath = "Images/" + image.FileName });
                        }
                        catch (Exception ex)
                        {
                           
                            Console.WriteLine($"Error copying image: {ex.Message}");
                        }
                    }
                }
                else
                {
                    product.ImagesSmall = new List<ProductImage>(); // Ensure collection is initialized
                }

                // Check if there are any images in ImagesSmall collection
                if (product.ImagesSmall != null && product.ImagesSmall.Any())
                {
                    // Set the Image property to the path of the first image
                    product.Image = product.ImagesSmall.First().ImagePath;
                }


                // Add the product to the database
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["save"] = "Product has been added";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, return the view with the model
            return View(product);
        }



        public IActionResult EditProduct(int Id)
        {
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");

         

            var product = _context.Products.Include(c => c.ProductTypes).Include(c => c.SpecialTag)
                .FirstOrDefault(c => c.Id == Id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);


        }


        [HttpPost]
        public async Task<IActionResult> EditProduct(Products product, List<IFormFile> ImagesSmall)
        {
            if (ModelState.IsValid)
            {


                var currentUser = await _userManager.GetUserAsync(User);

             
                var currentShop = await _context.FarmerShop.FirstOrDefaultAsync(shop => shop.FarmerUserId == currentUser.Id);
                if (currentShop != null)
                {
    
                    product.FarmerShopId = currentShop.Id;
                }

                product.CreationTime = DateTime.Now;
              
     
                if (product.ExpirationTime < DateTime.Now)
                {
                    ModelState.AddModelError(nameof(product.ExpirationTime), "Expiration time cannot be in the past.");
                    // Return the view with the validation error
                    return View(product);
                }

            


                if (ImagesSmall != null && ImagesSmall.Count > 0)
                {
                    product.ImagesSmall = new List<ProductImage>();

                    foreach (var image in ImagesSmall)
                    {
                        try
                        {
                            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images", Path.GetFileName(image.FileName));

                            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                            using (var fileStream = new FileStream(imagePath, FileMode.Create))
                            {
                                await image.CopyToAsync(fileStream);
                            }

                            product.ImagesSmall.Add(new ProductImage { ImagePath = "Images/" + image.FileName });
                        }
                        catch (Exception ex)
                        {
                      
                            Console.WriteLine($"Error copying image: {ex.Message}");
                        }
                    }
                }
                else
                {
                    product.ImagesSmall = new List<ProductImage>(); 
                }

            
                if (product.ImagesSmall != null && product.ImagesSmall.Any())
                {
    
                    product.Image = product.ImagesSmall.First().ImagePath;
                }


                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                TempData["save"] = "Product has been Updated";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }





        //GET Details Action Method
        public ActionResult ProductDetails(int? id)
        {
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
            ViewData["TagId"] = new SelectList(_context.SpecialTag.ToList(), "Id", "Name");

            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products
                .Include(p => p.ProductTypes)
                .Include(p => p.SpecialTag)
                .Include(p => p.ImagesSmall) // Include the collection of images
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        //GET Delete Action Method

        public ActionResult ProductDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.Include(c => c.SpecialTag).Include(c => c.ProductTypes).Where(c => c.Id == id).FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //POST Delete Action Method

        [HttpPost]
        [ActionName("ProductDelete")]
        public async Task<IActionResult> DeleteConfirm(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var product = _context.Products.FirstOrDefault(c => c.Id == Id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["save"] = "Product has been Deleted";
            return RedirectToAction(nameof(Index));
        }



        [AllowAnonymous]
        public async Task<IActionResult> Shop(int shopId)
        {
            // Check if the data is already cached
            if (!_cache.TryGetValue($"farmerShop_{shopId}", out FarmerShop farmerShop))
            {
                // Data is not in cache, so retrieve it from the database
                farmerShop = await _context.FarmerShop
                    .Include(fs => fs.Products)
                    .FirstOrDefaultAsync(fs => fs.Id == shopId);
                if (farmerShop == null)
                {
                    return NotFound(); // Shop not found
                }

                // Cache the data for 5 minutes
                _cache.Set($"farmerShop_{shopId}", farmerShop, TimeSpan.FromMinutes(5));
            }

            // Now retrieve products for the specific shop and categorize them
            var viewModel = new ShopViewModel
            {
                FarmerShop = farmerShop,
                Fruits = (List<Products>)GetCachedProductsByCategoryAndShop("Fruits", shopId),
                Vegetable = (List<Products>)GetCachedProductsByCategoryAndShop("Vegetable", shopId),
                Dairy = (List<Products>)GetCachedProductsByCategoryAndShop("Dairy", shopId),
                Pets = (List<Products>)GetCachedProductsByCategoryAndShop("Pets", shopId),
                Fish = (List<Products>)GetCachedProductsByCategoryAndShop("Fish", shopId),
                Meat = (List<Products>)GetCachedProductsByCategoryAndShop("Meat", shopId),
                Crops = (List<Products>)GetCachedProductsByCategoryAndShop("Crops", shopId),
                Cattle = (List<Products>)GetCachedProductsByCategoryAndShop("Cattle", shopId),
                Bird = (List<Products>)GetCachedProductsByCategoryAndShop("Bird", shopId),
                Honey = (List<Products>)GetCachedProductsByCategoryAndShop("Honey", shopId),
            };

            return View(viewModel);
        }

        private IEnumerable<Products> GetCachedProductsByCategoryAndShop(string category, int shopId)
        {
            string cacheKey = $"{category}_{shopId}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Products> cachedProducts))
            {
                // Data is not in cache, so retrieve it from the database
                cachedProducts = _context.Products
                    .Where(p => p.ProductTypes.ProductType == category && p.FarmerShopId == shopId) // Filter by category and shop ID
                    .Include(p => p.ProductTypes)
                    .Include(p => p.SpecialTag)
                    .ToList();

                // Cache the data for 5 minutes
                _cache.Set(cacheKey, cachedProducts, TimeSpan.FromMinutes(5));
            }

            return cachedProducts;
        }



        private int GetFarmerShopId(ApplicationUser currentUser)
        {
            var farmerShop = _context.FarmerShop.FirstOrDefault(shop => shop.FarmerUserId == currentUser.Id);
            return farmerShop?.Id ?? 0;
        }





        //Inventory

        // Action method to show all products in the shop
        public IActionResult ShowAllProducts(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            var productsInShop = farmerShop.GetAllProductsInShop();
            return View(productsInShop);
        }

        // Action method to show sold products
        public IActionResult ShowSoldProducts(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            var soldProducts = farmerShop.GetSoldProducts();
            return View(soldProducts);
        }

        // Action method to calculate and show total revenue
        public IActionResult ShowTotalRevenue(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            var totalRevenue = farmerShop.CalculateTotalRevenue();
            return View(totalRevenue);
        }

        // Action method to find and show the most sold product
        public IActionResult ShowMostSoldProduct(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            var mostSoldProduct = farmerShop.FindMostSoldProduct();
            return View(mostSoldProduct);
        }

        // Action method to show daily sales
        public IActionResult ShowDailySales(int farmerShopId)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            int dailySales = farmerShop.GetDailySales();
            return View(dailySales);
        }


        public IActionResult CashoutRevenue(int farmerShopId, CashoutInterval interval)
        {
            var farmerShop = _context.FarmerShop.Find(farmerShopId);
            if (farmerShop == null)
            {
                return NotFound();
            }

            // Calculate revenue for the specified interval
            decimal revenue = farmerShop.CalculateAndCashoutRevenue(interval);

            // Award bonuses for best-selling shops
            farmerShop.AwardBonusesForBestSellingShops();

            // Return the remaining revenue after platform fee deduction
            return View(revenue);
        }




    }
}

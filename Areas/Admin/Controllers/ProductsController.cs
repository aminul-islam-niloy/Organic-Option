using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View(_db.Products.Include(c => c.ProductTypes).Include(f => f.SpecialTag).ToList());
        }


        [HttpPost]
        public IActionResult Index(decimal? lowAmount, decimal? largeAmount, string searchQuery)
        {
            var products = _db.Products.Include(c => c.ProductTypes).Include(c => c.SpecialTag).ToList();

            if (lowAmount != null && largeAmount != null)
            {
                products = products.Where(c => c.Price >= lowAmount && c.Price <= largeAmount).ToList();
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(c => c.Name.Contains(searchQuery)).ToList();
            }

            return View(products);
        }

        public IActionResult SetDiscount()
        {
            return View();
        }


        [HttpPost]
        public IActionResult SetDiscount(Products product, bool isRamadan, bool isEid)
        {
            var products = _db.Products.Include(c => c.ProductTypes).Include(f => f.SpecialTag).ToList();


            foreach (var pro in products)
            {

                if (isRamadan)
                {
                    pro.Discount = 10;
                    pro.DiscountPrice -= 0.10M * pro.Price;
                }
                else if (isEid)
                {
                    pro.Discount = 15;
                    pro.DiscountPrice -= 0.15M * pro.Price;
                }
                else
                {
                    pro.Discount = 0;
                    pro.DiscountPrice = 0;
                }

            }

            _db.Products.UpdateRange(products);
            _db.SaveChanges();

            TempData["Discount"] = "Discount Has been Set";
            return RedirectToAction(nameof(Index));

        }

    }
}
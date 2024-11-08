using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OrganicOption.Models.Blogs;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrganicOption.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductPriceController : Controller
    {
  
        private readonly ApplicationDbContext _context;

        public ProductPriceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductPrice/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateProductTypesDropDownList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductPrice productPrice)
        {
            if (ModelState.IsValid)
            {
                productPrice.UpdatedDate = DateTime.Now;
                _context.Add(productPrice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateProductTypesDropDownList();
            return View(productPrice);
        }

        private void PopulateProductTypesDropDownList()
        {
            ViewData["productTypeId"] = new SelectList(_context.ProductTypes.ToList(), "Id", "ProductType");
        }


        // GET: ProductPrice/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            PopulateProductTypesDropDownList();
            var productPrice = await _context.ProductPrices.FindAsync(id);
            if (productPrice == null)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
           
            return View(productPrice);
        }

        // POST: ProductPrice/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductPrice productPrice)
        {
            if (id != productPrice.Id)
            {
                   return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
            }
            PopulateProductTypesDropDownList();
            if (ModelState.IsValid)
            {
                try
                {
                    productPrice.UpdatedDate = DateTime.Now;
                    _context.Update(productPrice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductPriceExists(productPrice.Id))
                    {
                           return RedirectToAction("ErrorPage", "Home", new { area = "Customer" });
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ProductTypes = _context.ProductTypes.ToListAsync();
            return View(productPrice);
        }

        // GET: ProductPrice
        public async Task<IActionResult> Index()
        {
            var productPrices = await _context.ProductPrices
                .Include(p => p.ProductType)
                .ToListAsync();

            var groupedProductPrices = productPrices
                .GroupBy(p => p.ProductType.ProductType)
                .ToList();

            return View(groupedProductPrices);
        }

        [AllowAnonymous]
        public async Task<IActionResult> TodayPrice()
        {
            var productPrices = await _context.ProductPrices
                .Include(p => p.ProductType)
                .ToListAsync();

            var groupedProductPrices = productPrices
                .GroupBy(p => p.ProductType.ProductType)
                .ToList();

            return View(groupedProductPrices);
        }

        private bool ProductPriceExists(int id)
        {
            return _context.ProductPrices.Any(e => e.Id == id);
        }
    }
}

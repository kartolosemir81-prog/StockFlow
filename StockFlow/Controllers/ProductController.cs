
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;

using ProductModel = global::StockFlow.Models.Product;

namespace StockFlow.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ürünleri listele, ara ve filtrele
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? stockFilter)
        {
            searchTerm = searchTerm?.Trim();

            stockFilter = stockFilter?.ToLowerInvariant();

            if (stockFilter != "instock" &&
                stockFilter != "low" &&
                stockFilter != "out")
            {
                stockFilter = "all";
            }

            // Genel istatistikler (filtrelerden etkilenmez)
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            ViewBag.TotalStock =
                await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;

            ViewBag.TotalCost =
                await _context.Products.SumAsync(
                    p => (decimal?)(p.PurchasePrice * p.StockQuantity)) ?? 0m;

            ViewBag.CriticalStock =
                await _context.Products.CountAsync(p => p.StockQuantity <= 5);

            // Ürün sorgusu
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            // Ürün adı veya barkod arama
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    (p.Barcode != null && p.Barcode.Contains(searchTerm)));
            }

            // Stok durumuna göre filtreleme
            switch (stockFilter)
            {
                case "instock":
                    query = query.Where(p => p.StockQuantity > 0);
                    break;

                case "low":
                    query = query.Where(p =>
                        p.StockQuantity > 0 && p.StockQuantity <= 5);
                    break;

                case "out":
                    query = query.Where(p => p.StockQuantity == 0);
                    break;
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm ?? "";
            ViewBag.StockFilter = stockFilter;

            return View(products);
        }

        // Ürün ekleme sayfası
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni ürün kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            if (!ModelState.IsValid)
                return View(product);

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ürün başarıyla eklendi.";

            return RedirectToAction(nameof(Index));
        }

        // Ürün düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // Ürün güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductModel product)
        {
            if (id != product.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(product);

            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
                return NotFound();

            existingProduct.ProductName = product.ProductName;
            existingProduct.Barcode = product.Barcode;
            existingProduct.PurchasePrice = product.PurchasePrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.StockQuantity = product.StockQuantity;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ürün başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }

        // Ürün sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ürün başarıyla silindi.";

            return RedirectToAction(nameof(Index));
        }
    }
}

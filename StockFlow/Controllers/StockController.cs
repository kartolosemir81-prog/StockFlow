
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Controllers
{
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new StockIndexViewModel();
            PrepareDashboard(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StockIndexViewModel model)
        {
            if (model.ProductId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.ProductId),
                    "Lütfen ürün seçin.");
            }

            if (model.MovementType != "Giriş" &&
                model.MovementType != "Çıkış")
            {
                ModelState.AddModelError(
                    nameof(model.MovementType),
                    "Geçersiz işlem türü.");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.Id == model.ProductId);

            if (product == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProductId),
                    "Seçilen ürün bulunamadı.");
            }
            else if (model.MovementType == "Çıkış" &&
                     model.Quantity > product.StockQuantity)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "Stokta yeterli ürün bulunmuyor.");
            }

            if (!ModelState.IsValid)
            {
                PrepareDashboard(model);
                return View("Index", model);
            }

            if (model.MovementType == "Giriş")
            {
                product!.StockQuantity += model.Quantity;
            }
            else
            {
                product!.StockQuantity -= model.Quantity;
            }

            var movement = new StockMovement
            {
                ProductId = product.Id,
                MovementType = model.MovementType,
                Quantity = model.Quantity,
                Description = model.Description,
                CreatedAt = DateTime.Now
            };

            _context.StockMovements.Add(movement);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Stok işlemi başarıyla kaydedildi.";

            return RedirectToAction(nameof(Index));
        }

        private void PrepareDashboard(StockIndexViewModel model)
        {
            model.Products = _context.Products
                .AsNoTracking()
                .OrderBy(p => p.ProductName)
                .ToList();

            model.Movements = _context.StockMovements
                .Include(m => m.Product)
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .ToList();

            model.TotalProducts = _context.Products.Count();

            model.TotalStock = _context.Products
                .Sum(p => (int?)p.StockQuantity) ?? 0;

            model.TotalEntries = _context.StockMovements
                .Where(m => m.MovementType == "Giriş")
                .Sum(m => (int?)m.Quantity) ?? 0;

            model.TotalExits = _context.StockMovements
                .Where(m => m.MovementType == "Çıkış")
                .Sum(m => (int?)m.Quantity) ?? 0;
        }
    }
}

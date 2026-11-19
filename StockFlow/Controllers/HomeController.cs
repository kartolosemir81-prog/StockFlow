
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;
using StockFlow.Models.ViewModels;

namespace StockFlow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            model.TotalProducts = await _context.Products.CountAsync();

            model.TotalStockQuantity =
                await _context.Products
                    .SumAsync(p => (int?)p.StockQuantity) ?? 0;

            model.CriticalStockCount =
                await _context.Products
                    .CountAsync(p => p.StockQuantity <= 5);

            model.TotalStockValue =
                await _context.Products
                    .SumAsync(p => (decimal?)(p.PurchasePrice * p.StockQuantity))
                ?? 0m;

            model.RecentProducts =
                await _context.Products
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new ProductSummaryViewModel
                    {
                        ProductName = p.ProductName,
                        StockQuantity = p.StockQuantity,
                        SalePrice = p.SalePrice,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

            model.RecentMovements =
                await _context.StockMovements
                    .AsNoTracking()
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(5)
                    .Select(m => new StockMovementSummaryViewModel
                    {
                        ProductName = m.Product != null
                            ? m.Product.ProductName
                            : "Bilinmeyen ürün",

                        MovementType = m.MovementType,
                        Quantity = m.Quantity,
                        CreatedAt = m.CreatedAt
                    })
                    .ToListAsync();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}

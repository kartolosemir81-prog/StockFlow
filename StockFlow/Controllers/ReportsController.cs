
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            DateTime end = endDate ?? DateTime.Today;

            if (start > end)
            {
                ModelState.AddModelError(
                    "",
                    "Başlangıç tarihi bitiş tarihinden sonra olamaz.");

                start = end;
            }

            var movements = _context.StockMovements
                .AsNoTracking()
                .Include(m => m.Product)
                .Where(m =>
                    m.CreatedAt >= start.Date &&
                    m.CreatedAt < end.Date.AddDays(1))
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            var products = _context.Products
                .AsNoTracking()
                .ToList();

            var model = new ReportsViewModel
            {
                StartDate = start,
                EndDate = end,

                TotalProducts = products.Count,

                TotalStock = products.Sum(p => p.StockQuantity),

                ZeroStockProducts = products.Count(
                    p => p.StockQuantity == 0),

                TotalStockValue = products.Sum(
                    p => p.PurchasePrice * p.StockQuantity),

                TotalEntries = movements
                    .Where(m => m.MovementType == "Giriş")
                    .Sum(m => m.Quantity),

                TotalExits = movements
                    .Where(m => m.MovementType == "Çıkış")
                    .Sum(m => m.Quantity),

                Movements = movements
            };

            var currentDate = start.Date;

            while (currentDate <= end.Date)
            {
                var dailyMovements = movements
                    .Where(m => m.CreatedAt.Date == currentDate)
                    .ToList();

                model.ChartLabels.Add(
                    currentDate.ToString("dd.MM"));

                model.EntryData.Add(
                    dailyMovements
                        .Where(m => m.MovementType == "Giriş")
                        .Sum(m => m.Quantity));

                model.ExitData.Add(
                    dailyMovements
                        .Where(m => m.MovementType == "Çıkış")
                        .Sum(m => m.Quantity));

                currentDate = currentDate.AddDays(1);
            }

            return View(model);
        }

        // Excel raporu indirme
        public IActionResult ExportExcel(
            DateTime? startDate,
            DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            DateTime end = endDate ?? DateTime.Today;

            if (start > end)
            {
                return BadRequest(
                    "Başlangıç tarihi bitiş tarihinden sonra olamaz.");
            }

            var movements = _context.StockMovements
                .AsNoTracking()
                .Include(m => m.Product)
                .Where(m =>
                    m.CreatedAt >= start.Date &&
                    m.CreatedAt < end.Date.AddDays(1))
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Stok Raporu");

            // Başlık
            worksheet.Cell("A1").Value = "STOCKFLOW STOK HAREKET RAPORU";
            worksheet.Range("A1:E1").Merge();

            worksheet.Cell("A2").Value =
                $"Rapor Tarihi: {start:dd.MM.yyyy} - {end:dd.MM.yyyy}";

            worksheet.Range("A2:E2").Merge();

            // Sütun başlıkları
            worksheet.Cell("A4").Value = "Ürün Adı";
            worksheet.Cell("B4").Value = "İşlem Türü";
            worksheet.Cell("C4").Value = "Miktar";
            worksheet.Cell("D4").Value = "Açıklama";
            worksheet.Cell("E4").Value = "Tarih";

            int row = 5;

            foreach (var movement in movements)
            {
                worksheet.Cell(row, 1).Value =
                    movement.Product?.ProductName ?? "Silinmiş ürün";

                worksheet.Cell(row, 2).Value =
                    movement.MovementType;

                worksheet.Cell(row, 3).Value =
                    movement.Quantity;

                worksheet.Cell(row, 4).Value =
                    movement.Description ?? "-";

                worksheet.Cell(row, 5).Value =
                    movement.CreatedAt.ToString("dd.MM.yyyy HH:mm");

                row++;
            }

            // Tasarım
            var title = worksheet.Range("A1:E1");
            title.Style.Font.Bold = true;
            title.Style.Font.FontSize = 16;
            title.Style.Font.FontColor = XLColor.White;
            title.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            title.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            var headers = worksheet.Range("A4:E4");
            headers.Style.Font.Bold = true;
            headers.Style.Font.FontColor = XLColor.White;
            headers.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");

            worksheet.Columns().AdjustToContents();

            worksheet.SheetView.FreezeRows(4);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StockFlow_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}

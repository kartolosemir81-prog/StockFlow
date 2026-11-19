
namespace StockFlow.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }

        public int TotalStockQuantity { get; set; }

        public int CriticalStockCount { get; set; }

        public decimal TotalStockValue { get; set; }

        public List<ProductSummaryViewModel> RecentProducts { get; set; } = new();

        public List<StockMovementSummaryViewModel> RecentMovements { get; set; } = new();
    }

    public class ProductSummaryViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StockMovementSummaryViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

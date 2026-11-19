
using StockFlow.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.ViewModels
{
    public class StockIndexViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();

        public List<StockMovement> Movements { get; set; } = new List<StockMovement>();

        public int TotalProducts { get; set; }

        public int TotalStock { get; set; }

        public int TotalEntries { get; set; }

        public int TotalExits { get; set; }

        [Required(ErrorMessage = "Lütfen ürün seçin.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Ýþlem türü seçin.")]
        public string MovementType { get; set; } = "Giriþ";

        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalýdýr.")]
        public int Quantity { get; set; } = 1;

        public string? Description { get; set; }
    }
}

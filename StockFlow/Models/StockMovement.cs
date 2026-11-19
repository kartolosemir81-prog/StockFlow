
using System;
using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public global::StockFlow.Models.Product? Product { get; set; }

        [Required]
        public string MovementType { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        public int Quantity { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

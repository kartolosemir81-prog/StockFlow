
using Microsoft.EntityFrameworkCore;

namespace StockFlow.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<global::StockFlow.Models.Product> Products { get; set; }

        public DbSet<global::StockFlow.Models.StockMovement> StockMovements { get; set; }
    }
}

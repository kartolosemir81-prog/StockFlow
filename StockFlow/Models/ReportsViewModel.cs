
using System;
using System.Collections.Generic;
using StockFlow.Models;

namespace StockFlow.Models
{
    public class ReportsViewModel
    {
        // Genel istatistikler
        public int TotalProducts { get; set; }

        public int TotalStock { get; set; }

        public int TotalEntries { get; set; }

        public int TotalExits { get; set; }

        public int ZeroStockProducts { get; set; }

        public decimal TotalStockValue { get; set; }

        // Tarih filtreleri
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // Stok hareketleri
        public List<StockMovement> Movements { get; set; }
            = new List<StockMovement>();

        // Grafik verileri
        public List<string> ChartLabels { get; set; }
            = new List<string>();

        public List<int> EntryData { get; set; }
            = new List<int>();

        public List<int> ExitData { get; set; }
            = new List<int>();
    }
}

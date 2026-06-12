using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("StockIn")]
    public class StockInModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ProductId { get; set; } // Link to ProductModels

        public double QuantityAdded { get; set; }

        // Tracks what you paid for THIS specific batch
        public decimal UnitCostAtTimeOfStock { get; set; }

        [Indexed]
        public int? VendorId { get; set; } // Who supplied it?

        public string ReferenceNumber { get; set; } = string.Empty; // Invoice or Receipt #

        public DateTime StockInDate { get; set; } = DateTime.Now;

        public string Remarks { get; set; } = string.Empty; // e.g., "Initial Load" or "Damage Replacement"
        [Indexed]
        public Guid SecureId { get; set; } = Guid.NewGuid();
        [Ignore]
        public double TotalCost => (double)((decimal)QuantityAdded * (decimal)UnitCostAtTimeOfStock);
        // Optional: Helpful for perishables
        public DateTime? BatchExpiryDate { get; set; }
    }
}
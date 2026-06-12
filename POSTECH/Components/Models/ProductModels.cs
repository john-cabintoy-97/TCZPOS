using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("Products")]
    public class ProductModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // --- Core Identification ---
        [Unique, MaxLength(50)] 
        public string Barcode { get; set; } = string.Empty;
        [Unique, MaxLength(50)] 
        public string Pcode { get; set; } = string.Empty;

        [Indexed, MaxLength(150)] 
        public string Name { get; set; } = string.Empty;

       
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Markup => SellingPrice - CostPrice;

        // ---  (Foreign Keys) ---
        [Indexed]
        public int CategoryId { get; set; }

        [Indexed]
        public int? BrandId { get; set; }   

        [Indexed]
        public int VendorId { get; set; }  
        // --- Inventory & Logistics ---
        public int StockLevel { get; set; }
        public double ReorderPoint { get; set; } = 10.0;

        public string Unit { get; set; } = "pcs"; // "kg", "pcs", "pack", "tray"

        // --- Perishables Management ---
        public bool HasExpiry { get; set; } = false;
        public DateTime? ExpiryDate { get; set; }

        // --- Status ---
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        [Indexed]
        public Guid SecureId { get; set; } = Guid.NewGuid();
        [Ignore]
        public bool IsLowStock => StockLevel <= ReorderPoint;
        [Ignore]
        public int TempQty { get; set; } = 1; // Default to 1
        [Ignore]
        public string StockStatusColor => IsLowStock ? "text-red-500" : "text-emerald-500";
        [Ignore]
        public int DaysUntilExpiry => ExpiryDate.HasValue ? (ExpiryDate.Value.Date - DateTime.Now.Date).Days : 9999;

        [Ignore]
        public string ExpiryStatusColor => DaysUntilExpiry switch
        {
            <= 0 => "bg-red-600 text-white",      // Already Expired
            <= 7 => "bg-orange-500 text-white",   // Critical (1 week)
            <= 30 => "bg-yellow-400 text-black",  // Warning (1 month)
            _ => "text-slate-500"                 // Safe
        };
    }
}
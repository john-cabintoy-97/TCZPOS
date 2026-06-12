using SQLite;

namespace TCZPOS.Components.Models
{
    public class PurchaseOrderModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty; // e.g., PO-2024-001
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Partially Received, Completed
        public bool IsReceived { get; set; }
        [Ignore] // Not saved in DB, populated via Service
        public List<PurchaseOrderItemModels> Items { get; set; } = new();
    }
}

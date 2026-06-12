using SQLite;

namespace TCZPOS.Components.Models
{
    public class PurchaseOrderItemModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityOrdered { get; set; }
        public int QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
        public decimal SubTotal => QuantityOrdered * UnitCost;
    }
}

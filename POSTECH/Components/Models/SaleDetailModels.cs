using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("SaleDetails")]
    public class SaleDetailModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int SaleId { get; set; } // Link to the Header (SaleModels.Id)

        [Indexed]
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => (decimal)Quantity * UnitPrice;

        // We store the CostPrice here to calculate PROFIT later
        // even if the product's current cost price changes in the future.
        public decimal CostPriceAtTimeOfSale { get; set; }
    }
}

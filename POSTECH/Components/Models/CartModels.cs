using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Models
{
    [Table("Cart")]
    public class CartModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string TransactionNo { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Pcode { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        // --- Discount Logic ---
        public decimal DiscountValue { get; set; } // e.g., 10 (for 10%) or 5 (for ₱5.00)
        public string DiscountType { get; set; } = "Percent"; // "Percent" or "Fixed"

        // New: Track if this is a "Global" discount applied to all items 
        // or a "Manual" discount applied only to this specific row.
        public string DiscountCategory { get; set; } = "Manual";

        [Ignore]
        public decimal ItemDiscountAmount
        {
            get
            {
                decimal qty = (decimal)Quantity;

                if (DiscountType == "Percent")
                {
                    // Calculation: (Total Price) * (Discount Rate)
                    return (UnitPrice * qty) * (DiscountValue / 100);
                }
                return DiscountValue;
            }
        }
        // Calculated on the fly in the UI
        [Ignore]
        public decimal SubTotal
        {
            get
            {
                decimal qty = (decimal)Quantity;

                // 2. Perform the calculation
                decimal basePrice = UnitPrice * qty;

                if (DiscountType == "Percent")
                {
                    // Calculate percentage discount
                    return basePrice - (basePrice * (DiscountValue / 100));
                }
                return basePrice - DiscountValue;
            }
        }
    }
}

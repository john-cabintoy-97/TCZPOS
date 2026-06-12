using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("Sales")]
    public class SaleModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, MaxLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty; // e.g., POS-20260324-001

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        [Ignore] public decimal NetAmount => TotalAmount - DiscountAmount + TaxAmount;
        public string PaymentType { get; set; } = "Cash"; // Cash, GCash, Card
        public decimal AmountPaid { get; set; }
        [Ignore]
        public decimal ChangeDue => AmountPaid > NetAmount ? AmountPaid - NetAmount : 0;
        public string CashierName { get; set; } = "System Admin";
        public int? CustomerId { get; set; } // Null for walk-in
        public string CustomerName { get; set; } = "Walk-in";
        public int? StaffId { get; set; } // Link to the Staff table
        public bool IsVoided { get; set; } = false;
        public string? VoidReason { get; set; }     
        public DateTime? VoidDate { get; set; }     
        public string? VoidedBy { get; set; }
        public string PaymentStatus { get; set; } = "Paid";
        public decimal CreditBalance { get; set; }
        public DateTime? DueDate { get; set; }
        [Ignore]
        public bool IsCredit => PaymentStatus == "Credit" || PaymentStatus == "Partial";
        [Ignore]
        public List<SaleDetailModels> Items { get; set; } = [];
    }
}

using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("CustomerCredits")]
    public class CustomerCreditModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public decimal TotalDebt { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Cleared
    }
}

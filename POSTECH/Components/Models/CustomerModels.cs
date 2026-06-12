using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("Customers")]
    public class CustomerModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; } = 5000; // Default limit
        public bool IsActive { get; set; } = true;
    }
}

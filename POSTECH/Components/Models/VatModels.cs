using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("VatSettings")]
    public class VatModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, MaxLength(50)]
        public string Name { get; set; } = "VAT Exempt"; // e.g., "Vatable", "Zero Rated"
        public double Percentage { get; set; } // e.g., 12.0
        public bool IsDefault { get; set; } = false;
        [Indexed]
        public Guid SecureId { get; set; } = Guid.NewGuid();
    }
}

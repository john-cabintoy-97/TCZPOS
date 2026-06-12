using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("HeldTransactions")]
    public class HeldTransactionModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string ReferenceName { get; set; } = string.Empty;
        public DateTime HeldAt { get; set; } = DateTime.Now;

        // SQLite will save this string
        public string SerializedItems { get; set; } = string.Empty;

        // Your UI will use this list (SQLite will ignore it)
        [Ignore]
        public List<CartModels> Items { get; set; } = new();

        public Guid SecureId { get; set; } = Guid.NewGuid();
    }
}
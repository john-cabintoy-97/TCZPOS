using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("Brands")]
    public class BrandModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        [Indexed]
        public Guid SecureId { get; set; } = Guid.NewGuid();
        public override string ToString() => Name;

    }
}

using SQLite;

namespace TCZPOS.Components.Models
{
    [Table("Categories")]
    public class CategoryModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ Unique, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;
        [Indexed]
        public Guid SecureId { get; set; } = Guid.NewGuid();
        public override string ToString() => Name;


    }
}

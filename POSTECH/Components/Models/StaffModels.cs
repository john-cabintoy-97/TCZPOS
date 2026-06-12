using SQLite;

namespace TCZPOS.Components.Models
{
    public enum StaffRole
    {
        Cashier,      // Can sell and view personal history
        Inventory,    // Can add products, adjust stock, and manage brands/categories
        AdminStaff,   // Can do everything except license/system management
        Owner         // The Master User (You)
    }

    [Table("Staff")]
    public class StaffModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]  
        public int MasterUserId { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(4)]
        public string PinCode { get; set; } = string.Empty; // 4-digit PIN for quick access

        public StaffRole Role { get; set; } = StaffRole.Cashier;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // This helps you track who is currently using the mobile/terminal
        [Ignore]
        public bool IsCurrentlyActiveSession { get; set; }
    }
}
using SQLite;
using System;
using System.Collections.Generic;

namespace TCZPOS.Components.Models
{
    [Table("AIProducts")]
    public class AIProductModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ShortName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [Indexed]
        public Guid SecureId { get; set; } = Guid.NewGuid();

        public int UsageCount { get; set; } = 0; // For popularity tracking

        public override string ToString() => $"{ShortName} ({FullName})";
    }

  
}


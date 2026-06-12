using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Models
{
    [Table("ProductLearningHistory")]
    public class ProductLearningHistoryModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ProductId { get; set; }

        public string OriginalInput { get; set; } = string.Empty;
        public bool WasAccepted { get; set; } = false;

        public string SuggestedName { get; set; } = string.Empty;

        public string AcceptedName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Indexed]
        public Guid SessionId { get; set; } = Guid.NewGuid();
    }
}

using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.DTOs
{
    public class ProductViewDTO
    {
        public ProductModels Product { get; set; } = new();
        public string CategoryName { get; set; } = "Uncategorized";
        public string BrandName { get; set; } = "No Brand";
        public string VendorName { get; set; } = "No Vendor";
    }
}

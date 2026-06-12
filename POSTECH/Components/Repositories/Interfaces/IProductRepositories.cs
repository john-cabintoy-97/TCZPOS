using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IProductRepositories
    {
        
        Task<List<ProductModels>> GetAllProductsAsync();
        Task<ProductModels?> GetProductByIdAsync(int id);
        Task<ProductModels?> GetProductByBarcodeAsync(string barcode);
        Task<ProductModels?> GetProductByPcodeAsync(string pcode);

        // Search & Filter
        Task<List<ProductModels>> SearchProductsAsync(string searchTerm);

        // Persistence
        Task<int> SaveProductAsync(ProductModels product);
        Task<int> DeleteProductAsync(ProductModels product);
        Task<bool> UpdateBatchPricesAsync(List<ProductModels> products);
        Task<int> DeleteMultipleProductsAsync(List<ProductModels> products);
        // Validation Checks
        Task<bool> ProductExistsAsync(string barcode, string pcode, int currentId);

        // Dashboard/Inventory Helpers
        Task<int> GetLowStockCountAsync();
        Task<double> GetTotalStockValueAsync();
        Task<bool> UpdateStockAsync(int productId, int quantityChange);
    }
}

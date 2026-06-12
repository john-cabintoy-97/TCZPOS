using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class ProductRepositories(DBQueries _db) : IProductRepositories
    {
        // 1. Get All
        public async Task<List<ProductModels>> GetAllProductsAsync()
        {
            return await _db.SelectAllAsync<ProductModels>();
        }

        // 2. Get by ID
        public async Task<ProductModels?> GetProductByIdAsync(int id)
        {
            var results = await _db.SelectFilteredAsync<ProductModels>(x => x.Id == id);
            return results.FirstOrDefault();
        }

        // 3. Get by Barcode (For Scanner)
        public async Task<ProductModels?> GetProductByBarcodeAsync(string barcode)
        {
            var results = await _db.SelectFilteredAsync<ProductModels>(x => x.Barcode == barcode && x.IsActive);
            return results.FirstOrDefault();
        }

        // 4. Get by Pcode (Internal Code)
        public async Task<ProductModels?> GetProductByPcodeAsync(string pcode)
        {
            var results = await _db.SelectFilteredAsync<ProductModels>(x => x.Pcode == pcode && x.IsActive);
            return results.FirstOrDefault();
        }

        // 5. Search (Case-Insensitive)
        public async Task<List<ProductModels>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return await GetAllProductsAsync();

            return await _db.SelectFilteredAsync<ProductModels>(x =>
                x.Name.ToLower().Contains(searchTerm.ToLower()) ||
                x.Barcode.Contains(searchTerm) ||
                x.Pcode.Contains(searchTerm));
        }

        // 6. Save (Insert or Update)
        public async Task<int> SaveProductAsync(ProductModels product)
        {
            if (product.Id == 0)
            {
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                return await _db.InsertAsync(product);
            }
            else
            {
                product.UpdatedAt = DateTime.Now;
                return await _db.UpdateAsync(product);
            }
        }

        // 7. Duplicate Check
        public async Task<bool> ProductExistsAsync(string barcode, string pcode, int currentId)
        {
            var results = await _db.SelectFilteredAsync<ProductModels>(x =>
                x.Id != currentId && (x.Barcode == barcode || x.Pcode == pcode));

            return results.Any();
        }

        // 8. Low Stock Calculation
        public async Task<int> GetLowStockCountAsync()
        {
            // Since we can't use [Ignore] in LINQ to SQL, we use the raw logic
            var results = await _db.SelectFilteredAsync<ProductModels>(x =>
                x.StockLevel <= x.ReorderPoint && x.IsActive);

            return results.Count;
        }

        // 9. Relationship Count (For safe delete in Brand/Vendor/Category)
        public async Task<int> GetTotalDataCountAsync(int id)
        {
            // We use Manual Query for specific ID checking across 3 columns
            var sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = ? OR BrandId = ? OR VendorId = ?";
            var result = await _db.QueryManualAsync<int>(sql, id, id, id);
            return result.FirstOrDefault();
        }

        // 10. Delete
        public async Task<int> DeleteProductAsync(ProductModels product)
        {
            return await _db.DeleteAsync(product);
        }

        public async Task<bool> UpdateBatchPricesAsync(List<ProductModels> products)
        {
            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    foreach (var product in products)
                    {
                        product.UpdatedAt = DateTime.Now;
                        conn.Update(product); 
                    }
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<double> GetTotalStockValueAsync()
        {
            var sql = "SELECT SUM(StockLevel * CostPrice) FROM Products WHERE IsActive = 1";

            try
            {
                var result = await _db.QueryManualAsync<double>(sql);
                return result.FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantityChange)
        {
            string sql = "UPDATE Products SET StockLevel = StockLevel + ?, UpdatedAt = ? WHERE Id = ?";
            var result = await _db.ExecuteAsync(sql, quantityChange, DateTime.Now, productId);
            return result > 0;
        }

        public async Task<int> DeleteMultipleProductsAsync(List<ProductModels> products)
        {
            if (products == null || !products.Any()) return 0;

            int totalDeleted = 0;

            // Use the helper you already built in DBQueries!
            await _db.ExecuteInTransactionAsync(conn =>
            {
                foreach (var product in products)
                {
                    // Inside the transaction, we use the synchronous Delete (tran.Delete)
                    // 'conn' here is the SQLiteConnection provided by your DBQueries
                    totalDeleted += conn.Delete(product);
                }
            });

            return totalDeleted;
        }
    
    }
}
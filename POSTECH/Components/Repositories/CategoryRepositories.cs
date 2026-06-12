using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class CategoryRepositories(DBQueries _db) : ICategoryRepositories
    {
        public async Task<bool> CategoryExistsAsync(string name)
        {
            var results = await _db.SelectFilteredAsync<CategoryModels>(x => x.Name == name);
            return results.Any();
        }

        public async Task<int> DeleteCategoryAsync(CategoryModels category)
        {
            return await _db.DeleteAsync(category);
        }

        public async Task<List<CategoryModels>> GetAllCategoriesAsync()
        {
            return await _db.SelectAllAsync<CategoryModels>();

        }

        public async Task<CategoryModels?> GetCategoryByIdAsync(int id)
        {
            var results = await _db.SelectFilteredAsync<CategoryModels>(x => x.Id == id);
            return results.FirstOrDefault();
        }

        public async Task<int> GetTotalProductCountAsync(int categoryId)
        {
            var sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = ?";
            var result = await _db.QueryManualAsync<int>(sql, categoryId);
            return result.FirstOrDefault();
        }

        public async Task<int> SaveCategoryAsync(CategoryModels category)
        {
            if (category.Id == 0)
            {
                category.CreatedAt = DateTime.Now;
                return await _db.InsertAsync(category);
            }
            else
            {
                category.UpdatedAt = DateTime.Now;
                return await _db.UpdateAsync(category);
            }
        }

        

        public async Task<int> DeleteMultipleSafeAsync(List<CategoryModels> categories)
        {
            if (categories == null || !categories.Any()) return 0;

            int totalDeleted = 0;

            // We use the internal transaction logic from your DBQueries if available, 
            // or we can execute a custom transaction block here.
            await _db.ExecuteInTransactionAsync(conn =>
            {
                foreach (var category in categories)
                {
                    // SAFETY CHECK: Check for linked products before deleting
                    // Note: Inside a transaction, we use synchronous calls
                    var productCount = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Products WHERE CategoryId = ?", category.Id);

                    if (productCount > 0)
                    {
                        // Throwing an exception inside RunInTransactionAsync 
                        // triggers an automatic ROLLBACK.
                        throw new Exception($"Category '{category.Name}' is not empty.");
                    }

                    // Perform the delete
                    totalDeleted += conn.Delete(category);
                }
            });

            return totalDeleted;
        }
    }
}

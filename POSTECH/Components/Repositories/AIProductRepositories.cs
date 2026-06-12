using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class AIProductRepositories(DBQueries _db) : IAIProductRepositories
    {
        public async Task<List<AIProductModels>> GetAllDataAsync()
        {
            return await _db.SelectFilteredAsync<AIProductModels>(x => x.IsActive);
        }

        public async Task<AIProductModels?> GetDataByIdAsync(int id)
        {
            var results = await _db.SelectFilteredAsync<AIProductModels>(x => x.Id == id && x.IsActive);
            return results.FirstOrDefault();
        }

        public async Task<AIProductModels?> GetDataByFullNameAsync(string fullName)
        {
            var results = await _db.SelectFilteredAsync<AIProductModels>(x => x.FullName == fullName && x.IsActive);
            return results.FirstOrDefault();
        }

        public async Task<List<AIProductModels>> SearchProductsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllDataAsync();

            // Use manual SQL for better search performance
            string searchQuery = $"%{query.ToUpperInvariant()}%";
            var sql = @"SELECT * FROM AIProducts 
                       WHERE (UPPER(FullName) LIKE ? OR UPPER(ShortName) LIKE ?) 
                       AND IsActive = 1 
                       ORDER BY UsageCount DESC 
                       LIMIT 20";

            return await _db.QueryManualAsync<AIProductModels>(sql, searchQuery, searchQuery);
        }

        public async Task<List<AIProductModels>> GetPopularProductsAsync(int limit = 10)
        {
            var sql = @"SELECT * FROM AIProducts 
                       WHERE IsActive = 1 
                       ORDER BY UsageCount DESC, CreatedAt DESC 
                       LIMIT ?";

            return await _db.QueryManualAsync<AIProductModels>(sql, limit);
        }

        public async Task<bool> DataExistsAsync(string fullName)
        {
            var results = await _db.SelectFilteredAsync<AIProductModels>(x => x.FullName == fullName);
            return results.Any();
        }

        public async Task<int> SaveDataAsync(AIProductModels data)
        {
            string normalizedName = data.FullName.Trim().ToUpperInvariant();

            var existing = await GetDataByFullNameAsync(normalizedName);

            if (existing != null && data.Id == 0)
            {
                 existing.ShortName = data.ShortName;
                existing.UpdatedAt = DateTime.Now;
                existing.UsageCount++;
                return await _db.UpdateAsync(existing);
            }

            if (data.Id == 0)
            {
                data.CreatedAt = DateTime.Now;
                data.SecureId = Guid.NewGuid();
                return await _db.InsertAsync(data);
            }
            else
            {
                data.UpdatedAt = DateTime.Now;
                return await _db.UpdateAsync(data);
            }
        }

        public async Task<int> DeleteDataAsync(AIProductModels data)
        {
            // Soft delete
            data.IsActive = false;
            data.UpdatedAt = DateTime.Now;
            return await _db.UpdateAsync(data);
        }

        public async Task<int> GetTotalDataCountAsync()
        {
            var sql = "SELECT COUNT(*) FROM AIProducts WHERE IsActive = 1";
            var result = await _db.QueryManualAsync<int>(sql);
            return result.FirstOrDefault();
        }

        public async Task<int> IncrementUsageCountAsync(int productId)
        {
            var sql = "UPDATE AIProducts SET UsageCount = UsageCount + 1, UpdatedAt = ? WHERE Id = ?";
            return await _db.ExecuteAsync(sql, DateTime.Now, productId);
        }

        // Learning History Methods
        public async Task<int> SaveLearningHistoryAsync(ProductLearningHistoryModels history)
        {
            history.CreatedAt = DateTime.Now;
            return await _db.InsertAsync(history);
        }

        public async Task<List<ProductLearningHistoryModels>> GetUnacceptedSuggestionsAsync()
        {
            return await _db.SelectFilteredAsync<ProductLearningHistoryModels>(x => !x.WasAccepted);
        }

        public async Task<int> UpdateLearningHistoryAsync(ProductLearningHistoryModels history)
        {
            return await _db.UpdateAsync(history);
        }
    }
}
using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class VendorRepositories(DBQueries _db) : IVendorRepositories
    {
        public async Task<bool> DataExistsAsync(string name)
        {
            var results = await _db.SelectFilteredAsync<BrandModels>(x => x.Name == name);
            return results.Any();
        }

        public async Task<int> DeleteDataAsync(VendorModels data)
        {
            return await _db.DeleteAsync(data);
        }

        public async Task<int> DeleteMultipleSafeAsync(List<VendorModels> data)
        {
            if (data == null || !data.Any()) return 0;

            int totalDeleted = 0;

            await _db.ExecuteInTransactionAsync(conn =>
            {
                foreach (var list in data)
                {
                    var productCount = conn.ExecuteScalar<int>(
                       "SELECT COUNT(*) FROM Products WHERE BrandId = ?", list.Id);

                    if (productCount > 0)
                    {
                        throw new Exception($"Brand '{list.Name}' is not empty.");
                    }

                    totalDeleted += conn.Delete(list);
                }
            });

            return totalDeleted;

        }

        public async Task<List<VendorModels>> GetAllDataAsync()
        {
            return await _db.SelectAllAsync<VendorModels>();
        }

        public async Task<VendorModels?> GetDataByIdAsync(int id)
        {
            var results = await _db.SelectFilteredAsync<VendorModels>(x => x.Id == id);
            return results.FirstOrDefault();
        }

        public async Task<int> GetTotalDataCountAsync(int dataId)
        {
            var sql = "SELECT COUNT(*) FROM Products WHERE VendorId = ?";
            var result = await _db.QueryManualAsync<int>(sql, dataId);
            return result.FirstOrDefault();
        }

        public async Task<int> SaveDataAsync(VendorModels data)
        {
            if (data.Id == 0)
            {
                data.CreatedAt = DateTime.Now;
                return await _db.InsertAsync(data);
            }
            else
            {
                data.UpdatedAt = DateTime.Now;
                return await _db.UpdateAsync(data);
            }
        }
    }
}

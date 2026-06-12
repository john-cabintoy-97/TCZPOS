using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using TCZPOS.Components.Database;
using System.Text.Json;

namespace TCZPOS.Components.Repositories
{
    public class HeldTransactionRepository(DBQueries _db) : IHeldTransactionRepository
    {
        public async Task<int> SaveHeldTransactionAsync(HeldTransactionModel model)
        {
            model.SerializedItems = JsonSerializer.Serialize(model.Items);

            return await _db.InsertAsync(model);
        }

        public async Task<List<HeldTransactionModel>> GetHeldTransactionsAsync()
        {
            var results = await _db.SelectAllAsync<HeldTransactionModel>();

            foreach (var item in results)
            {
                if (!string.IsNullOrEmpty(item.SerializedItems))
                {
                    item.Items = JsonSerializer.Deserialize<List<CartModels>>(item.SerializedItems) ?? new();
                }
            }

            return results.OrderByDescending(x => x.HeldAt).ToList();
        }

        public async Task<int> DeleteHeldTransactionAsync(int id)
        {
             string sql = "DELETE FROM HeldTransactions WHERE Id = ?";
            return await _db.ExecuteAsync(sql, id);
        }
    }
}
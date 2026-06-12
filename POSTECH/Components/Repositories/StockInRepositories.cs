using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories
{
    public class StockInRepositories(DBQueries _db) : IStockInRepositories
    {
        public async Task<int> AddStockInAsync(StockInModels stockIn)
        {
            return await _db.InsertAsync(stockIn);
        }

        public async Task<int> AddStockInBatchAsync(List<StockInModels> stockInList)
        {
            if (stockInList == null || stockInList.Count == 0) return 0;

            int rowsAffected = 0;

            await _db.ExecuteInTransactionAsync(conn =>
            {
                foreach (var item in stockInList)
                {
                    rowsAffected += conn.Insert(item);
                    string updateSql = "UPDATE Products SET StockLevel = StockLevel + ? WHERE Id = ?";
                    conn.Execute(updateSql, item.QuantityAdded, item.ProductId);
                }
            });

            return rowsAffected;
        }

        public async Task<List<StockInModels>> GetStockInByReferenceAsync(string referenceNo)
        {
            return await _db.SelectFilteredAsync<StockInModels>(x => x.ReferenceNumber == referenceNo);
        }

        public async Task<List<StockInModels>> GetStockInHistoryAsync()
        {
            var results = await _db.SelectAllAsync<StockInModels>();
            return results.OrderByDescending(x => x.StockInDate).ToList();
        }

        public async Task<int> UpdateProductStockAsync(int productId, double quantityAdded)
        {
            string sql = "UPDATE ProductModels SET StockLevel = StockLevel + @0 WHERE Id = @1";
            return await _db.ExecuteAsync(sql, quantityAdded, productId);
        }
    }
}

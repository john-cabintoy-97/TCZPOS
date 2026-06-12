using TCZPOS.Components.Models;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IStockInRepositories
    {
        Task<int> UpdateProductStockAsync(int productId, double quantityAdded);
        Task<int> AddStockInAsync(StockInModels stockIn);
        Task<int> AddStockInBatchAsync(List<StockInModels> stockInList);
        Task<List<StockInModels>> GetStockInHistoryAsync();
        Task<List<StockInModels>> GetStockInByReferenceAsync(string referenceNo);
    }
}

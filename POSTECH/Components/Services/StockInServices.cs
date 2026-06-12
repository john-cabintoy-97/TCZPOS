using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class StockInServices(IStockInRepositories _repository)
    {
        public async Task<bool> AddSingleStockInAsync(StockInModels item)
        {
            try
            {
                // Ensure business rules are met
                if (item.SecureId == Guid.Empty) item.SecureId = Guid.NewGuid();
                if (item.StockInDate == default) item.StockInDate = DateTime.Now;

                int result = await _repository.AddStockInAsync(item);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Single StockIn Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessStockInBatchAsync(List<StockInModels> items, StockInModels header)
        {
            try
            {
                if (items == null || items.Count == 0) return false;

                foreach (var item in items)
                {
                    item.ReferenceNumber = header.ReferenceNumber;
                    item.VendorId = header.VendorId;
                    item.StockInDate = header.StockInDate;
                    item.Remarks = header.Remarks;

                    if (item.SecureId == Guid.Empty) item.SecureId = Guid.NewGuid();
                }

                int result = await _repository.AddStockInBatchAsync(items);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Batch StockIn Error: {ex.Message}");
                return false;
            }
        }


        public async Task<List<StockInModels>> GetHistoryAsync()
        {
            return await _repository.GetStockInHistoryAsync();
        }

        public async Task<List<StockInModels>> GetByReferenceAsync(string refNo)
        {
            if (string.IsNullOrWhiteSpace(refNo)) return new List<StockInModels>();
            return await _repository.GetStockInByReferenceAsync(refNo);
        }
    }
}
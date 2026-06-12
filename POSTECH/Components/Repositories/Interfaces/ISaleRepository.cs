using TCZPOS.Components.Models;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface ISaleRepository
    {
        Task<bool> VoidSaleWithTransactionAsync (SaleModels sale, List<SaleDetailModels> details, bool restoreStock);

        Task<bool> CreateSaleAsync(SaleModels sale, List<SaleDetailModels> details);
        Task<List<SaleModels>> GetAllSalesAsync();
        Task<List<SaleModels>> GetDailySalesAsync(DateTime date);
        Task<List<SaleDetailModels>> GetSaleDetailsAsync(int saleId);
        Task<bool> VoidSingleItemAsync(int saleId, SaleDetailModels item, decimal amountToDeduct, int qtyToRemove);
        Task<SaleModels?> GetSaleByInvoiceAsync(string invoice);
    }
}

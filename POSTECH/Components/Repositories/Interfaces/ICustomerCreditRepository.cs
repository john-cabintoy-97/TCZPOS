using TCZPOS.Components.DTOs;
using TCZPOS.Components.Models;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface ICustomerCreditRepository
    {
        Task<List<SaleModels>> GetUnpaidSalesByCustomerAsync(int customerId);
        Task<int> UpdateSalePaymentAsync(SaleModels sale);
        Task<List<SaleModels>> GetAllCreditRecordsAsync();
        Task<List<CustomerCreditModels>> GetAllCreditSummariesAsync();
        Task<CustomerCreditModels?> GetSummaryByCustomerIdAsync(int customerId);
        Task<bool> UpdatePaymentTransactionAsync(SaleModels sale, CustomerCreditModels summary);
        Task<List<SaleModels>> GetSalesByCustomerAsync(int customerId);
        Task<bool> UpdateMasterSummaryAsync(CustomerCreditModels summary);
    }
}

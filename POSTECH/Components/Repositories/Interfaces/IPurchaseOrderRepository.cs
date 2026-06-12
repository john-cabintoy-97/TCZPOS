using TCZPOS.Components.Models;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<List<PurchaseOrderModels>> GetAllOrdersAsync();
        Task<PurchaseOrderModels> GetOrderByIdAsync(int id);
        Task<bool> SaveOrderWithItemsAsync(PurchaseOrderModels order);
        Task<bool> UpdateStatusAsync(int orderId, string status);
        Task<bool> DeleteOrderAsync(int id);
    }
}

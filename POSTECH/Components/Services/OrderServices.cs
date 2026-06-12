using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using TCZPOS.Components.Extension;

namespace TCZPOS.Components.Services
{
    public class OrderServices(IPurchaseOrderRepository repo, ProductServices productRepo)
    {
        public List<PurchaseOrderModels> OrderList { get; private set; } = new();

        public async Task<List<PurchaseOrderModels>> GetAllOrdersAsync()
        {
            OrderList = await repo.GetAllOrdersAsync();
            return OrderList;
        }

        public async Task<ResponseHelper> SaveOrderAsync(PurchaseOrderModels order)
        {
            if (order.Items == null || !order.Items.Any())
            {
                return new ResponseHelper { Success = false, Message = "Cannot save an empty order." };
            }

            bool success = await repo.SaveOrderWithItemsAsync(order);

            if (success)
            {
                await GetAllOrdersAsync();
                return new ResponseHelper { Success = true, Message = "Order saved successfully!" };
            }

            return new ResponseHelper { Success = false, Message = "Database error occurred." };
        }

        public async Task<ResponseHelper> UpdateOrderStatusAsync(PurchaseOrderModels order)
        {
            if (order.Status == "Completed")
            {
                return new ResponseHelper { Success = false, Message = "Order is already processed and stock has been updated." };
            }
            bool isSuccess = await repo.UpdateStatusAsync(order.Id, "Completed");

            if (isSuccess)
            {
                // 2. If status change was successful, trigger the inventory math
                await ProcessInventoryUpdate(order.Id);

                // 3. Refresh the local list so the UI updates
                await GetAllOrdersAsync();
                return new ResponseHelper { Success = true, Message = "Order status updated successfully." };
            }
            return new ResponseHelper { Success = false, Message = "Failed to update order status." };
        }

        private async Task ProcessInventoryUpdate(int orderId)
        {
            // 1. Fetch the full order (including Items) from the repository
            var fullOrder = await repo.GetOrderByIdAsync(orderId);

            if (fullOrder?.Items != null)
            {
                foreach (var item in fullOrder.Items)
                {
                     await productRepo.UpdateStockAsync(item.ProductId, item.QuantityOrdered);
                }
            }
        }

        public async Task<ResponseHelper> DeleteOrderAsync(int id)
        {
            bool success = await repo.DeleteOrderAsync(id);
            if (success)
            {
                await GetAllOrdersAsync(); // Refresh local list
                return new ResponseHelper { Success = true, Message = "Order deleted." };
            }
            return new ResponseHelper { Success = false, Message = "Failed to delete order." };
        }

        public async Task<PurchaseOrderModels?> GetOrderByIdAsync(int id)
        {
            return await repo.GetOrderByIdAsync(id);
        }
    }
}
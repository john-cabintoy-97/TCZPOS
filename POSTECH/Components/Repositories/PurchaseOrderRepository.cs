using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class PurchaseOrderRepository(DBQueries dbQueries) : IPurchaseOrderRepository
    {
        public async Task<List<PurchaseOrderModels>> GetAllOrdersAsync()
        {
            return await dbQueries.SelectAllAsync<PurchaseOrderModels>();
        }

        public async Task<PurchaseOrderModels> GetOrderByIdAsync(int id)
        {
            var order = await dbQueries.GetAsync<PurchaseOrderModels>(id);
            if (order != null)
            {
                order.Items = await dbQueries.SelectFilteredAsync<PurchaseOrderItemModels>(x => x.PurchaseOrderId == id);
            }
            return order;
        }

        public async Task<bool> SaveOrderWithItemsAsync(PurchaseOrderModels order)
        {
            try
            {
                await dbQueries.RunTransactionAsync(conn =>
                {
                    // 1. Save or Update the Header
                    if (order.Id == 0) conn.Insert(order);
                    else conn.Update(order);
                    conn.Execute("DELETE FROM PurchaseOrderItemModels WHERE PurchaseOrderId = ?", order.Id);

                    // 3. Insert fresh items and UPDATE STOCK
                    foreach (var item in order.Items)
                    {
                        item.PurchaseOrderId = order.Id;
                        conn.Insert(item);
                        if (!order.IsReceived)
                        {
                            string updateSql = "UPDATE Products SET StockLevel = StockLevel + ? WHERE Id = ?";
                            conn.Execute(updateSql, item.QuantityOrdered, item.ProductId);
                        }
                    }
                    if (!order.IsReceived)
                    {
                        order.IsReceived = true;
                        conn.Update(order);
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Receiving Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStatusAsync(int orderId, string status)
        {
            var order = await dbQueries.GetAsync<PurchaseOrderModels>(orderId);
            if (order == null) return false;

            order.Status = status;
            int rows = await dbQueries.UpdateAsync(order);
            return rows > 0;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                await dbQueries.RunTransactionAsync(conn =>
                {
                    // Delete child items first to avoid foreign key constraints
                    conn.Execute("DELETE FROM PurchaseOrderItemModels WHERE PurchaseOrderId = ?", id);
                    // Delete the main order
                    conn.Execute("DELETE FROM PurchaseOrderModels WHERE Id = ?", id);
                });
                return true;
            }
            catch { return false; }
        }
    }
}
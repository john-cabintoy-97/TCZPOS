using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using QRCoder;

namespace TCZPOS.Components.Repositories
{
    public class CartRepositories(DBQueries _db) : ICartRepositories
    {
        public async Task<int> AddOrUpdateCartItemAsync(CartModels item)
        {
            var existingItems = await _db.SelectFilteredAsync<CartModels>(i => i.ProductId == item.ProductId);
            var existingItem = existingItems.FirstOrDefault();

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                existingItem.UnitPrice = item.UnitPrice;
                existingItem.DiscountValue = item.DiscountValue;
                existingItem.DiscountType = item.DiscountType;
                return await _db.UpdateAsync(existingItem);
            }

            return await _db.InsertAsync(item);
        }

        public async Task<int> ClearCartAsync()
        {
            return await _db.ExecuteAsync("DELETE FROM Cart");
        }

        public async Task<List<CartModels>> GetCartItemsAsync()
        {
            var items = await _db.SelectAllAsync<CartModels>();
            return [..  items.OrderByDescending(x => x.Id)];
        }

        public async Task<decimal> GetTotalSalesAsync()
        {
            var items = await GetCartItemsAsync();
            return items.Sum(x => x.SubTotal);
        }

        public async Task<int> UpdateQuantityAsync(int id, int exactQty)
        {
            // Fetch the specific row by ID
            var item = await _db.GetAsync<CartModels>(id);
            if (item != null)
            {
                item.Quantity = exactQty; // OVERWRITE (No +=)
                return await _db.UpdateAsync(item);
            }
            return 0;
        }
        public async Task<int> RemoveItemAsync(int id)
        {
            return await _db.DeleteAsync(new CartModels { Id = id });
        }

        public async Task<int> UpdateItemAsync(CartModels item)
        {
            return await _db.UpdateAsync(item);
        }
    }
}

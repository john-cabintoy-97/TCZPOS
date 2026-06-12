using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class CartServices(ICartRepositories _cartRepo)
    {
        public event Action? OnCartChanged;

        public async Task<List<CartModels>> GetItemsAsync() => await _cartRepo.GetCartItemsAsync();

        public async Task AddItemAsync(CartModels item)
        {
            await _cartRepo.AddOrUpdateCartItemAsync(item);
            NotifyStateChanged();
        }

        public async Task RemoveItemAsync(int id)
        {
            await _cartRepo.RemoveItemAsync(id);
            NotifyStateChanged();
        }

        public async Task ClearAsync()
        {
            await _cartRepo.ClearCartAsync();
            NotifyStateChanged();
        }

        // --- MATH CALCULATIONS FOR YOUR FOOTER ---
        public async Task SetItemQuantityAsync(int id, int exactQty)
        {
            if (exactQty <= 0)
            {
                await RemoveItemAsync(id);
            }
            else
            {
                await _cartRepo.UpdateQuantityAsync(id, exactQty);
                NotifyStateChanged();
            }
        }

        public async Task<decimal> GetSubTotalAsync()
        {
            var items = await GetItemsAsync();
            return items.Sum(item =>
            {
                // Explicitly cast Quantity (double) to decimal
                decimal qty = (decimal)item.Quantity;

                decimal basePrice = item.UnitPrice * qty;

                if (item.DiscountType == "Percent")
                {
                    return basePrice - (basePrice * (item.DiscountValue / 100));
                }

                return basePrice - item.DiscountValue;
            });
        }

        public async Task<decimal> GetVatAmountAsync()
        {
            var total = await GetSubTotalAsync();
            // Assuming 12% VAT as seen in your UI
            return total * 0.12m;
        }

        public async Task<decimal> GetNetTotalAsync()
        {
            // Total - Discount + VAT (or however your local business logic requires)
            return await GetSubTotalAsync();
        }


        public async Task<string> GenerateNextTransactionNoAsync()
        {
            string currentPrefix = DateTime.Now.ToString("yyyyMMddHH");

            var allItems = await _cartRepo.GetCartItemsAsync();

            if (allItems == null || !allItems.Any())
            {
                return $"{currentPrefix}1001";
            }

            int maxSuffix = allItems
                .Where(p => !string.IsNullOrWhiteSpace(p.TransactionNo) && p.TransactionNo.StartsWith(currentPrefix))
                .Select(p =>
                {
                    string suffixPart = p.TransactionNo.Substring(10);
                    return int.TryParse(suffixPart, out int num) ? num : 1000;
                })
                .DefaultIfEmpty(1000)
                .Max();

            return $"{currentPrefix}{(maxSuffix + 1)}";
        }

        public async Task UpdateItemAsync(CartModels item)
        {
            await _cartRepo.UpdateItemAsync(item);
            NotifyStateChanged();
        }


        private void NotifyStateChanged() => OnCartChanged?.Invoke();
    }
}
 using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface ICartRepositories
    {
        Task<List<CartModels>> GetCartItemsAsync();
        Task<int> AddOrUpdateCartItemAsync(CartModels item);
        Task<int> UpdateQuantityAsync(int id, int exactQty); 
        Task<int> RemoveItemAsync(int id);
        Task<int> ClearCartAsync();
        Task<decimal> GetTotalSalesAsync();

        Task<int> UpdateItemAsync(CartModels item);

    }
}

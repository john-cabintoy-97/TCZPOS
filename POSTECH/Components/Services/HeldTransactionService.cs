using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Services
{
    public class HeldTransactionService(IHeldTransactionRepository _repo)
    {
        public async Task<bool> HoldCartAsync(string reference, List<CartModels> items)
        {
            if (items == null || !items.Any()) return false;

            var model = new HeldTransactionModel
            {
                ReferenceName = reference,
                Items = items,
                HeldAt = DateTime.Now
            };

            var result = await _repo.SaveHeldTransactionAsync(model);
            return result > 0;
        }

        public async Task<List<HeldTransactionModel>> GetAllHeldAsync() =>
            await _repo.GetHeldTransactionsAsync();

        public async Task RemoveHeldAsync(int id) =>
            await _repo.DeleteHeldTransactionAsync(id);
    }
}

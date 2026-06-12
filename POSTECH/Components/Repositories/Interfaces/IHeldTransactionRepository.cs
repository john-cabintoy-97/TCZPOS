using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IHeldTransactionRepository
    {
        Task<int> SaveHeldTransactionAsync(HeldTransactionModel model);
        Task<List<HeldTransactionModel>> GetHeldTransactionsAsync();
        Task<int> DeleteHeldTransactionAsync(int id);
    }
}

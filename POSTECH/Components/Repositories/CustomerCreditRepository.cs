using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using SQLite;

namespace TCZPOS.Components.Repositories
{
    public class CustomerCreditRepository(DBQueries _db) : ICustomerCreditRepository
    {
        public async Task<List<SaleModels>> GetUnpaidSalesByCustomerAsync(int customerId) =>
        await _db.SelectFilteredAsync<SaleModels>(s => s.CustomerId == customerId && s.CreditBalance > 0);

        public async Task<List<SaleModels>> GetAllCreditRecordsAsync()
        {
            return await _db.SelectFilteredAsync<SaleModels>(s =>
        s.PaymentStatus == "Credit" ||
        s.PaymentStatus == "Partial" ||
        s.PaymentStatus == "Paid");
        }

        public async Task<int> UpdateSalePaymentAsync(SaleModels sale)
        {
            return await _db.UpdateAsync(sale);
        }

        public async Task<bool> CanCustomerAvailCredit(int customerId, decimal newSaleAmount)
        {
            var customer = await _db.GetAsync<CustomerModels>(customerId);
            if (customer == null) return false;

            // Get current total debt
            var sales = await _db.SelectFilteredAsync<SaleModels>(s =>
                s.CustomerId == customerId && s.CreditBalance > 0);

            decimal currentDebt = sales.Sum(x => x.CreditBalance);

            // Check if new utang exceeds their limit
            return (currentDebt + newSaleAmount) <= customer.CreditLimit;
        }

        public async Task<List<CustomerCreditModels>> GetAllCreditSummariesAsync()
        {
            // Fetches all records from the CustomerCredits table
            return await _db.SelectAllAsync<CustomerCreditModels>();
        }

        public async Task<CustomerCreditModels?> GetSummaryByCustomerIdAsync(int customerId)
        {
            var results = await _db.SelectFilteredAsync<CustomerCreditModels>(cc => cc.CustomerId == customerId);
            return results.FirstOrDefault();
        }

        public async Task ExecuteTransactionAsync(Action<SQLiteConnection> action) =>
        await _db.ExecuteInTransactionAsync(action);

        public async Task<bool> UpdatePaymentTransactionAsync(SaleModels sale, CustomerCreditModels summary)
        {
            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    conn.Update(sale);    
                    conn.Update(summary);  
                });
                return true;
            }
            catch { return false; }
        }

        public async Task<List<SaleModels>> GetSalesByCustomerAsync(int customerId)
        {
            return await _db.SelectFilteredAsync<SaleModels>(s =>
                s.CustomerId == customerId &&
                (s.PaymentStatus == "Credit" || s.PaymentStatus == "Partial" || s.PaymentStatus == "Paid" && s.AmountPaid > 0 && s.PaymentType != "Cash"));
        }

        public async Task<bool> UpdateMasterSummaryAsync(CustomerCreditModels summary)
        {
            try
            {
                await _db.UpdateAsync(summary);
                return true;
            }
            catch { return false; }
        }
    }
}

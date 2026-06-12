using TCZPOS.Components.DTOs;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class CustomerCreditService(ICustomerCreditRepository _repo, ICustomerRepository _customerRepo, ISaleRepository _saleRepo) 
    {

        public async Task<List<CustomerCreditViewDTO>> GetDetailedCreditListAsync()
        {
            // 1. Fetch all customers from the database
            var customers = await _customerRepo.GetAllCustomersAsync();

            // 2. Fetch all credit summary records (TotalDebt, Status, etc.)
            var creditSummaries = await _repo.GetAllCreditSummariesAsync();

            // 3. Map and join them into the DTO
            return [.. customers.Select(c =>
            {
                var summary = creditSummaries.FirstOrDefault(s => s.CustomerId == c.Id);

                return new CustomerCreditViewDTO
                {
                    CustomerId = c.Id,
                    Name = c.Name,
                    ContactNumber = c.ContactNumber,
                    CreditLimit = c.CreditLimit,
                    // If no summary exists yet, their balance is 0
                    CurrentBalance = summary?.TotalDebt ?? 0
                };
            })];
        }
        public async Task<List<SaleDetailModels>> GetItemsBySaleIdAsync(int saleId)
        {
            return await _saleRepo.GetSaleDetailsAsync(saleId);
        }
        public async Task<List<SaleModels>> GetCreditHistoryAsync()
        {
            return await _repo.GetAllCreditRecordsAsync();
        }

        public async Task<decimal> GetTotalCustomerDebtAsync(int customerId)
        {
            var unpaidSales = await _repo.GetUnpaidSalesByCustomerAsync(customerId);
            return unpaidSales.Sum(x => x.CreditBalance);
        }

        public async Task<bool> RecordPaymentAsync(int saleId, decimal paymentAmount, string cashierName)
        {
            // Retrieve the specific unpaid sale record
            var allSales = await _repo.GetAllCreditRecordsAsync();
            var sale = allSales.FirstOrDefault(s => s.Id == saleId);

            if (sale == null || sale.CustomerId == null) return false;

            // Retrieve the master debt summary
            var summary = await _repo.GetSummaryByCustomerIdAsync(sale.CustomerId.Value);
            if (summary == null) return false;

            decimal appliedAmount = Math.Min(paymentAmount, sale.CreditBalance);

            // Update individual Sale balance
            sale.AmountPaid += appliedAmount;
            sale.CreditBalance -= appliedAmount;
            sale.PaymentStatus = sale.CreditBalance <= 0 ? "Paid" : "Partial";

            // Update Customer's total debt summary
            summary.TotalDebt -= appliedAmount;
            summary.LastPaymentDate = DateTime.Now;
            summary.Status = summary.TotalDebt <= 0 ? "Cleared" : "Active";

            // Commit changes via the repository transaction logic
            return await _repo.UpdatePaymentTransactionAsync(sale, summary);
        }

        public async Task<CustomerCreditViewDTO> GetCustomerLifetimeSummaryAsync(int customerId)
        {
            // Use the method that returns a List<SaleModels>
            var sales = await _repo.GetSalesByCustomerAsync(customerId);
            var customer = await _customerRepo.GetCustomerByIdAsync(customerId);

            if (customer == null) return new CustomerCreditViewDTO();

            return new CustomerCreditViewDTO
            {
                CustomerId = customerId,
                Name = customer.Name,
                // Now Sum works because 'sales' is an IEnumerable/List
                TotalPurchased = sales.Sum(s => s.NetAmount),
                TotalPaid = sales.Sum(s => s.AmountPaid),
                CurrentBalance = sales.Sum(s => s.CreditBalance)
            };
        }
        public async Task SyncCustomerDebtAsync(int customerId)
        {
            // 1. Get all credit records for this specific customer
            var allHistory = await _repo.GetAllCreditRecordsAsync();
            var customerSales = allHistory.Where(s => s.CustomerId == customerId).ToList();

            // 2. Calculate the REAL balance from individual invoices
            // Note: Use Math.Max(0, ...) if you want to prevent negative debt entirely
            decimal actualDebt = customerSales.Sum(s => s.CreditBalance);

            // 3. Get the Master Summary record
            var summary = await _repo.GetSummaryByCustomerIdAsync(customerId);

            if (summary != null)
            {
                // Update existing record
                summary.TotalDebt = actualDebt;
                summary.Status = actualDebt <= 0 ? "Cleared" : "Active";
                summary.LastPaymentDate = DateTime.Now;

                await _repo.UpdateMasterSummaryAsync(summary);
            }
            else if (actualDebt > 0)
            {
                // If no summary exists but there is debt, you might want to create one here
                // (Optional: logic to create new CustomerCreditModels if missing)
            }
        }
    }
}

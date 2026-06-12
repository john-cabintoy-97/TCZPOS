using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class SaleRepository(DBQueries _db) : ISaleRepository
    {
        public async Task<bool> CreateSaleAsync(SaleModels sale, List<SaleDetailModels> details)
        {
            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    conn.Insert(sale);

                    foreach (var detail in details)
                    {
                        detail.SaleId = sale.Id;
                        conn.Insert(detail);

                        string sql = "UPDATE Products SET StockLevel = StockLevel - ? WHERE Id = ?";
                        int rowsAffected = conn.Execute(sql, (double)detail.Quantity, (int)detail.ProductId);

                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"[Warning] No product found with ID: {detail.ProductId}");
                        }
                    }

                    if (sale.IsCredit && sale.CustomerId.HasValue)
                    {
                        // Try to find an existing credit record for this customer
                        var summary = conn.Table<CustomerCreditModels>()
                                          .FirstOrDefault(cc => cc.CustomerId == sale.CustomerId.Value);

                        if (summary != null)
                        {
                            summary.TotalDebt += sale.CreditBalance;
                            summary.Status = "Active";
                            conn.Update(summary);
                        }
                        else
                        {
                             var newSummary = new CustomerCreditModels
                            {
                                CustomerId = sale.CustomerId.Value,
                                CustomerName = sale.CustomerName,
                                TotalDebt = sale.CreditBalance,
                                Status = "Active",
                                LastPaymentDate = DateTime.Now 
                            };
                            conn.Insert(newSummary);
                        }
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaleRepository] Transaction Failed: {ex.Message}");
                return false;
            }
        }
        public async Task<List<SaleModels>> GetAllSalesAsync()
        {
            return await _db.SelectAllAsync<SaleModels>();
        }

        public async Task<List<SaleModels>> GetDailySalesAsync(DateTime date)
        {
            var start = date.Date;
            var end = date.Date.AddDays(1).AddTicks(-1);

            return await _db.SelectFilteredAsync<SaleModels>(s =>
                 s.TransactionDate >= start && s.TransactionDate <= end);
        }

        public async Task<SaleModels?> GetSaleByInvoiceAsync(string invoice)
        {
            var results = await _db.SelectFilteredAsync<SaleModels>(s => s.InvoiceNumber == invoice);
            return results.FirstOrDefault();
        }

        public async Task<List<SaleDetailModels>> GetSaleDetailsAsync(int saleId)
        {
            return await _db.SelectFilteredAsync<SaleDetailModels>(d => d.SaleId == saleId);
        }

        public async Task<bool> VoidSaleWithTransactionAsync(SaleModels sale, List<SaleDetailModels> details, bool restoreStock)
        {
            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    // This now saves IsVoided, VoidReason, VoidDate, and VoidedBy
                    conn.Update(sale);

                    if (restoreStock)
                    {
                        foreach (var item in details)
                        {
                            string sql = "UPDATE Products SET StockLevel = StockLevel + ? WHERE Id = ?";
                            conn.Execute(sql, item.Quantity, item.ProductId);
                        }
                    }

                    if (sale.IsCredit && sale.CustomerId.HasValue)
                    {
                        var summary = conn.Table<CustomerCreditModels>()
                                          .FirstOrDefault(cc => cc.CustomerId == sale.CustomerId.Value);
                        if (summary != null)
                        {
                            summary.TotalDebt -= sale.CreditBalance;
                            if (summary.TotalDebt < 0) summary.TotalDebt = 0; // Safety check
                            summary.Status = summary.TotalDebt <= 0 ? "Cleared" : "Active";
                            conn.Update(summary);
                        }
                    }
                });
                return true;
            }
            catch { return false; }

        }

        public async Task<bool> VoidSingleItemAsync(int saleId, SaleDetailModels item, decimal amountToDeduct, int qtyToRemove)
        {
            try
            {
                await _db.ExecuteInTransactionAsync(conn =>
                {
                    // 1. Handle the Item Quantity (Delete or Update)
                    if (qtyToRemove >= item.Quantity)
                    {
                        conn.Delete(item);
                    }
                    else
                    {
                        string updateDetailSql = "UPDATE SaleDetails SET Quantity = Quantity - ? WHERE Id = ?";
                        conn.Execute(updateDetailSql, qtyToRemove, item.Id);
                    }

                    // 2. Update the Sales Table (Total and Balance)
                    // Note: We also subtract from CreditBalance because the utang itself is smaller now
                    string updateSaleSql = @"UPDATE Sales SET 
                                     TotalAmount = TotalAmount - ?, 
                                     CreditBalance = CASE WHEN PaymentStatus IN ('Credit', 'Partial') 
                                                          THEN CreditBalance - ? 
                                                          ELSE CreditBalance END 
                                     WHERE Id = ?";
                    conn.Execute(updateSaleSql, (double)amountToDeduct, (double)amountToDeduct, saleId);

                    // 3. Update the Product Inventory
                    string updateStockSql = "UPDATE Products SET StockLevel = StockLevel + ? WHERE Id = ?";
                    conn.Execute(updateStockSql, qtyToRemove, item.ProductId);

                    // 4. Update the Customer's Total Debt Ledger
                    var sale = conn.Table<SaleModels>().FirstOrDefault(s => s.Id == saleId);
                    if (sale != null && sale.IsCredit && sale.CustomerId.HasValue)
                    {
                        var summary = conn.Table<CustomerCreditModels>()
                                          .FirstOrDefault(cc => cc.CustomerId == sale.CustomerId.Value);

                        if (summary != null)
                        {
                            summary.TotalDebt -= amountToDeduct;
                            if (summary.TotalDebt < 0) summary.TotalDebt = 0; // Prevent negative debt
                            summary.Status = summary.TotalDebt <= 0 ? "Cleared" : "Active";
                            conn.Update(summary);
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaleRepository] Item Void Failed: {ex.Message}");
                return false;
            }
        }
    }
}
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class SaleServices(ISaleRepository _saleRepo)
    {
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        private void StartLoading() { IsLoading = true; ErrorMessage = null; }
        private void StopLoading() => IsLoading = false;

        public async Task<List<SaleModels>> GetDailySalesAsync(DateTime? date = null)
        {
            try
            {
                StartLoading();
                var targetDate = date ?? DateTime.Today;
                var sales = await _saleRepo.GetDailySalesAsync(targetDate);

                foreach (var sale in sales)
                {
                    sale.Items = await _saleRepo.GetSaleDetailsAsync(sale.Id);
                }

                return [.. sales.OrderByDescending(s => s.TransactionDate)];
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load daily sales: {ex.Message}";
                return [];
            }
            finally { StopLoading(); }
        }

        public async Task<bool> ProcessFinalSaleAsync(SaleModels sale, List<CartModels> cartItems)
        {
            try
            {
                StartLoading();
                var saleDetails = cartItems.Select(item => new SaleDetailModels
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Barcode = item.Barcode,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CostPriceAtTimeOfSale = 0
                }).ToList();

                return await _saleRepo.CreateSaleAsync(sale, saleDetails);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Transaction failed: {ex.Message}";
                return false;
            }
            finally { StopLoading(); }
        }

        public async Task<List<SaleModels>> GetTransactionHistoryAsync()
        {
            try
            {
                var sales = await _saleRepo.GetAllSalesAsync();

                foreach (var sale in sales)
                {
                    sale.Items = await _saleRepo.GetSaleDetailsAsync(sale.Id);
                }

                return sales;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"History sync failed: {ex.Message}";
                return [];
            }
            finally { StopLoading(); }
        }

        public async Task<bool> VoidTransactionAsync(SaleModels sale, string? reason, bool restoreStock)
        {
            try
            {
                StartLoading();
                sale.IsVoided = true;
                var details = await _saleRepo.GetSaleDetailsAsync(sale.Id);
                return await _saleRepo.VoidSaleWithTransactionAsync(sale, details, restoreStock);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Void process failed: {ex.Message}";
                return false;
            }
            finally { StopLoading(); }
        }

        public async Task<bool> VoidSingleItemAsync(int saleId, SaleDetailModels item, int qtyToRemove)
        {
            try
            {
                StartLoading();
                decimal amountToDeduct = (decimal)(qtyToRemove * (double)item.UnitPrice);
                return await _saleRepo.VoidSingleItemAsync(saleId, item, amountToDeduct, qtyToRemove);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Item void failed: {ex.Message}";
                return false;
            }
            finally { StopLoading(); }
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            // Usually we don't show a global loader for ID generation 
            // to avoid UI "flicker", but we still need error handling.
            try
            {
                var today = DateTime.Now.ToString("yyyyMMdd");
                var allSales = await _saleRepo.GetAllSalesAsync();

                var lastSale = allSales
                                .Where(s => s.InvoiceNumber.StartsWith($"POS-{today}"))
                                .OrderByDescending(s => s.Id)
                                .FirstOrDefault();

                int nextSequence = 1;
                if (lastSale != null)
                {
                    var parts = lastSale.InvoiceNumber.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastId))
                    {
                        nextSequence = lastId + 1;
                    }
                }
                return $"POS-{today}-{nextSequence:D4}";
            }
            catch
            {
                return $"POS-{DateTime.Now:yyyyMMdd}-ERR";
            }
        }
    }
}
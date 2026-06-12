using FluentValidation;
using TCZPOS.Components.Models;
using TCZPOS.Components.DTOs;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class ProductServices(
        IProductRepositories _repo,
        IValidator<ProductModels> _validator,
        CategoryServices _categoryService,
        BrandServices _brandService,
        VendorServices _vendorService)
    {
        public List<ProductViewDTO> ProductDisplayList { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                if (!_categoryService.Categories.Any()) await _categoryService.LoadCategoriesAsync();
                if (!_brandService.BrandList.Any()) await _brandService.LoadAsync();
                if (!_vendorService.DataList.Any()) await _vendorService.LoadAsync();

                var products = await _repo.GetAllProductsAsync();

                ProductDisplayList = products.Select(p => new ProductViewDTO
                {
                    Product = p,
                    CategoryName = _categoryService.Categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Uncategorized",
                    BrandName = _brandService.BrandList.FirstOrDefault(b => b.Id == p.BrandId)?.Name ?? "No Brand",
                    VendorName = _vendorService.DataList.FirstOrDefault(v => v.Id == p.VendorId)?.Name ?? "No Vendor" // Use DataList here
                }).OrderBy(x => x.Product.Name).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Inventory sync failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<(bool Success, string Message)> SaveProductAsync(ProductModels product)
        {
            //if (string.IsNullOrWhiteSpace(product.Pcode) || product.Id == 0)
            //{
            //    product.Pcode = await GenerateNextPcodeAsync();
            //}

            var validationResult = await _validator.ValidateAsync(product);
            if (!validationResult.IsValid)
                return (false, validationResult.Errors.First().ErrorMessage);

            var exists = await _repo.ProductExistsAsync(product.Barcode, product.Pcode, product.Id);
            if (exists)
                return (false, "A product with this Barcode or Pcode already exists.");

            var result = await _repo.SaveProductAsync(product);

            if (result > 0)
            {
                await LoadProductsAsync();
                return (true, "Product saved successfully!");
            }

            return (false, "Database refused the entry.");
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(ProductModels product)
        {

            var result = await _repo.DeleteProductAsync(product);
            if (result > 0)
            {
                var dtoToRemove = ProductDisplayList.FirstOrDefault(x => x.Product.Id == product.Id);
                if (dtoToRemove != null) ProductDisplayList.Remove(dtoToRemove);
                return (true, "Product removed.");
            }
            return (false, "Delete failed.");
        }

        public async Task<(bool Success, string Message)> DeleteMultipleProductsAsync(List<ProductViewDTO> selectedDtos)
        {
            try
            {
                if (selectedDtos == null || !selectedDtos.Any())
                    return (false, "No products selected for deletion.");

                var productsToDelete = selectedDtos.Select(x => x.Product).ToList();

                var successCount = await _repo.DeleteMultipleProductsAsync(productsToDelete);

                ProductDisplayList.RemoveAll(d => productsToDelete.Any(p => p.Id == d.Product.Id));

                return (true, $"{successCount} products successfully removed from inventory.");
            }
            catch (Exception ex)
            {
                return (false, $"Batch Delete Failed: {ex.Message}");
            }
        }

        public async Task<string> GenerateNextPcodeAsync()
        {
            var allProducts = await _repo.GetAllProductsAsync();

            if (allProducts == null || !allProducts.Any())
            {
                return "P001";
            }

            int maxNumber = allProducts
                .Select(p =>
                {
                    if (string.IsNullOrWhiteSpace(p.Pcode) || !p.Pcode.StartsWith("P"))
                        return 0;

                    if (int.TryParse(p.Pcode.AsSpan(1), out int num))
                    {
                        return num;
                    }
                    else
                    {
                        return 0;
                    }

                })
                .Max();

            return $"P{(maxNumber + 1):D3}";
        }

        public async Task<bool> UpdateBatchPricesAsync(List<ProductModels> updates)
        {
            return await _repo.UpdateBatchPricesAsync(updates);
        }

        public ProductViewDTO? GetProductBySecureId(Guid secureId)
        {
            return ProductDisplayList.FirstOrDefault(x => x.Product.SecureId == secureId);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            try
            {
                var success = await _repo.UpdateStockAsync(productId, quantity);
                if (success)
                {
                    // Refresh the display list so the user sees the new stock levels
                    await LoadProductsAsync();
                }
                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Stock update failed: {ex.Message}";
                return false;
            }
        }

        public async Task<(bool Success, string Message)> SaveMultipleProductsAsync(List<ProductModels> products)
        {
            try
            {
                if (products == null || !products.Any())
                    return (false, "No items selected to update.");

                // Using the same logic as your Price Batch Update repository call
                // but for full model updates (Stock = 0, IsActive = false)
                var success = await _repo.UpdateBatchPricesAsync(products);

                if (success)
                {
                    await LoadProductsAsync(); // Refresh UI list
                    return (true, $"{products.Count} items successfully processed.");
                }

                return (false, "The database failed to update the records.");
            }
            catch (Exception ex)
            {
                return (false, $"Batch Update Failed: {ex.Message}");
            }
        }

        public List<ProductViewDTO> ExpiryWatchlist => [.. ProductDisplayList
        .Where(x => x.Product.HasExpiry && x.Product.ExpiryDate.HasValue)
        .OrderBy(x => x.Product.ExpiryDate)];

        public int ExpiringSoonCount => ExpiryWatchlist.Count(x =>
        (x.Product.ExpiryDate.Value.Date - DateTime.Now.Date).Days <= 30);
    }

}
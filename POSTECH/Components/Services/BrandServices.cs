using FluentValidation;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class BrandServices(IBrandRepositories _repo, IValidator<BrandModels> _validator)
    {
        public List<BrandModels> BrandList { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                var data = await _repo.GetAllDataAsync();
                BrandList = [.. data.OrderBy(x => x.Name)];
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to load brands. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 2. Add or Update with Validation
        public async Task<(bool Success, string Message)> SaveAsync(BrandModels data)
        {
            var validationResult = await _validator.ValidateAsync(data);

            if (!validationResult.IsValid)
            {
                // Returns the first specific error (e.g., "Brand name is too short.")
                return (false, validationResult.Errors.First().ErrorMessage);
            }

            var exists = await _repo.DataExistsAsync(data.Name);
            if (exists && data.Id == 0)
                return (false, $"The brand '{data.Name}' already exists.");

            var result = await _repo.SaveDataAsync(data);

            if (result > 0)
            {
                await LoadAsync(); // Refresh the local list
                return (true, "Brand saved successfully!");
            }

            return (false, "An error occurred while saving to the database.");
        }

        // 3. Delete with Safety Check
        public async Task<(bool Success, string Message)> DeleteAsync(BrandModels data)
        {
            // Optional: Check if products are linked to this category first
            var productCount = await _repo.GetTotalDataCountAsync(data.Id);
            if (productCount > 0)
            {
                return (false, $"Cannot delete. This brand contains {productCount} products.");
            }

            var result = await _repo.DeleteDataAsync(data);
            if (result > 0)
            {
                BrandList.Remove(data);
                return (true, "Brand deleted.");

            }

            return (false, "Delete failed.");
        }

        public async Task<(bool Success, string Message)> DeleteMultipleAsync(List<BrandModels> listData)
        {
            try
            {
                var count = await _repo.DeleteMultipleSafeAsync(listData);

                // Update your local list so the UI refreshes
                foreach (var cat in listData)
                {
                    BrandList.Remove(cat);
                }

                return (true, $"{count} brands successfully decommissioned.");
            }
            catch (Exception ex)
            {
                return (false, $"Batch Delete Failed: {ex.Message}");
            }
        }
    }
}


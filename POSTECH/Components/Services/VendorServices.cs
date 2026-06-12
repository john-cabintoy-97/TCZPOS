using FluentValidation;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class VendorServices(IVendorRepositories _repo, IValidator<VendorModels> _validator)
    {
        public List<VendorModels> DataList { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                var data = await _repo.GetAllDataAsync();
                DataList = [.. data.OrderBy(x => x.Name)];
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


        public async Task<(bool Success, string Message)> SaveAsync(VendorModels data)
        {
            var validationResult = await _validator.ValidateAsync(data);

            if (!validationResult.IsValid)
            {
                return (false, validationResult.Errors.First().ErrorMessage);
            }

            // Check for duplicates before saving
            var exists = await _repo.DataExistsAsync(data.Name);
            if (exists && data.Id == 0)
                return (false, $"The vendor '{data.Name}' already exists.");

            var result = await _repo.SaveDataAsync(data);

            if (result > 0)
            {
                await LoadAsync();
                return (true, "Vendor saved successfully!");
            }

            return (false, "An error occurred while saving to the database.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(VendorModels data)
        {
            // Optional: Check if products are linked to this category first
            var productCount = await _repo.GetTotalDataCountAsync(data.Id);
            if (productCount > 0)
            {
                return (false, $"Cannot delete. This vendor contains {productCount} products.");
            }

            var result = await _repo.DeleteDataAsync(data);
            if (result > 0)
            {
                DataList.Remove(data);
                return (true, "Brand deleted.");

            }

            return (false, "Delete failed.");
        }

        public async Task<(bool Success, string Message)> DeleteMultipleAsync(List<VendorModels> listData)
        {
            try
            {
                var count = await _repo.DeleteMultipleSafeAsync(listData);

                foreach (var cat in listData)
                {
                    DataList.Remove(cat);
                }

                return (true, $"{count} vendor successfully decommissioned.");
            }
            catch (Exception ex)
            {
                return (false, $"Batch Delete Failed: {ex.Message}");
            }
        }

    }
}

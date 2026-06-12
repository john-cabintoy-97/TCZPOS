using FluentValidation;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using TCZPOS.Components.Services.Hardware;

namespace TCZPOS.Components.Services
{
    public class CategoryServices(ICategoryRepositories _repo, IValidator<CategoryModels> _validator)
    {
        public List<CategoryModels> Categories { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        // 1. Load All Categories (Sorted Alphabetically)
        public async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                var data = await _repo.GetAllCategoriesAsync();
                Categories = [.. data.OrderBy(x => x.Name)];
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to load categories. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 2. Add or Update with Validation
        public async Task<(bool Success, string Message)> SaveCategoryAsync(CategoryModels category)
        {
            var validationResult = await _validator.ValidateAsync(category);

            if (!validationResult.IsValid)
            {
                // Returns the first error (e.g., "Category name must be at least 2 characters.")
                return (false, validationResult.Errors.First().ErrorMessage);
            }
            // Check for duplicates before saving
            var exists = await _repo.CategoryExistsAsync(category.Name);
            if (exists && category.Id == 0)
                return (false, $"The category '{category.Name}' already exists.");

            var result = await _repo.SaveCategoryAsync(category);

            if (result > 0)
            {
                await LoadCategoriesAsync(); // Refresh the local list
                return (true, "Category saved successfully!");
            }

            return (false, "An error occurred while saving to the database.");
        }

        // 3. Delete with Safety Check
        public async Task<(bool Success, string Message)> DeleteCategoryAsync(CategoryModels category)
        {
            // Optional: Check if products are linked to this category first
            var productCount = await _repo.GetTotalProductCountAsync(category.Id);
            if (productCount > 0)
            {
                return (false, $"Cannot delete. This category contains {productCount} products.");
            }

            var result = await _repo.DeleteCategoryAsync(category);
            if (result > 0)
            {
                Categories.Remove(category);
                return (true, "Category deleted.");

            }

            return (false, "Delete failed.");
        }

        public async Task<(bool Success, string Message)> DeleteMultipleCategoriesAsync(List<CategoryModels> categories)
        {
            try
            {
                var count = await _repo.DeleteMultipleSafeAsync(categories);

                // Update your local list so the UI refreshes
                //foreach (var cat in categories)
                //{
                //    Categories.Remove(cat);
                //}
                var idsToRemove = categories.Select(x => x.Id).ToList();
                var toRemove = Categories.Where(c => idsToRemove.Contains(c.Id)).ToList();
                foreach (var item in toRemove)
                {
                    Categories.Remove(item);
                }
                return (true, $"{count} categories successfully decommissioned.");
            }
            catch (Exception ex)
            {
                // This catches the "Category 'X' is not empty" error from the Repo
                return (false, $"Batch Delete Failed: {ex.Message}");
            }
        }
    }
}
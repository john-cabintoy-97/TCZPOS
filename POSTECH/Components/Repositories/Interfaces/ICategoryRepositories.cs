using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface ICategoryRepositories
    {
        Task<int> DeleteMultipleSafeAsync(List<CategoryModels> categories);
        Task<List<CategoryModels>> GetAllCategoriesAsync();
        Task<CategoryModels?> GetCategoryByIdAsync(int id);
        Task<int> SaveCategoryAsync(CategoryModels category); 
        Task<int> DeleteCategoryAsync(CategoryModels category);

        Task<bool> CategoryExistsAsync(string name);
        Task<int> GetTotalProductCountAsync(int categoryId); 
    }
}

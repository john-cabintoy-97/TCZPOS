using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IAIProductRepositories 
    {
        Task<List<AIProductModels>> GetAllDataAsync();
        Task<AIProductModels?> GetDataByIdAsync(int id);
        Task<AIProductModels?> GetDataByFullNameAsync(string fullName);
        Task<List<AIProductModels>> SearchProductsAsync(string query);
        Task<List<AIProductModels>> GetPopularProductsAsync(int limit = 10);
        Task<bool> DataExistsAsync(string fullName);
        Task<int> SaveDataAsync(AIProductModels data);
        Task<int> DeleteDataAsync(AIProductModels data);
        Task<int> GetTotalDataCountAsync();
        Task<int> IncrementUsageCountAsync(int productId);

        // Learning History
        Task<int> SaveLearningHistoryAsync(ProductLearningHistoryModels history);
        Task<List<ProductLearningHistoryModels>> GetUnacceptedSuggestionsAsync();
        Task<int> UpdateLearningHistoryAsync(ProductLearningHistoryModels history);

    }
}

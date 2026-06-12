using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IBrandRepositories
    {
        Task<int> DeleteMultipleSafeAsync(List<BrandModels> data);
        Task<List<BrandModels>> GetAllDataAsync();
        Task<BrandModels?> GetDataByIdAsync(int id);
        Task<int> SaveDataAsync(BrandModels data);
        Task<int> DeleteDataAsync(BrandModels data);

        Task<bool> DataExistsAsync(string name);
        Task<int> GetTotalDataCountAsync(int dataId);

    }
}

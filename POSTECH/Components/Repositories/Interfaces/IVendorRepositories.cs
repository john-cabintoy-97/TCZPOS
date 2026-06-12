using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IVendorRepositories
    {
        Task<int> DeleteMultipleSafeAsync(List<VendorModels> data);
        Task<List<VendorModels>> GetAllDataAsync();
        Task<VendorModels?> GetDataByIdAsync(int id);
        Task<int> SaveDataAsync(VendorModels data);
        Task<int> DeleteDataAsync(VendorModels data);
        Task<bool> DataExistsAsync(string name);
        Task<int> GetTotalDataCountAsync(int dataId);

    }
}

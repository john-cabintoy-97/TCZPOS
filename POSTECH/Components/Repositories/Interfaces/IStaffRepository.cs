using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<List<StaffModels>> GetStaffByMasterAccountAsync(int masterUserId);
        Task<StaffModels?> GetByPinAsync(int masterUserId, string pin);
        Task<int> SaveStaffAsync(StaffModels staff);
        Task<bool> DeleteStaffAsync(int staffId);
        Task<StaffModels?> GetByIdAsync(int id);
    }
}

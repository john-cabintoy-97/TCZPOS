using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Repositories
{
    public class StaffRepository(DBQueries _db) : IStaffRepository
    {
        public async Task<List<StaffModels>> GetStaffByMasterAccountAsync(int masterUserId)
        {
            // Now we only get staff belonging to THIS specific owner
            return await _db.SelectFilteredAsync<StaffModels>(s =>
                s.MasterUserId == masterUserId && s.IsActive);
        }

        public async Task<StaffModels?> GetByPinAsync(int masterUserId, string pin)
        {
            // Security check: Pin must match AND belong to the current Master Account
            var results = await _db.SelectFilteredAsync<StaffModels>(s =>
                s.MasterUserId == masterUserId &&
                s.PinCode == pin &&
                s.IsActive);

            return results.FirstOrDefault();
        }

        public async Task<StaffModels?> GetByIdAsync(int id)
        {
            return await _db.GetAsync<StaffModels>(id);
        }

        public async Task<int> SaveStaffAsync(StaffModels staff)
        {
            if (staff.Id != 0)
            {
                return await _db.UpdateAsync(staff);
            }
            else
            {
                return await _db.InsertAsync(staff);
            }
        }

        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            var staff = await GetByIdAsync(staffId);
            if (staff != null)
            {
                // Soft delete is usually better for POS systems to keep history
                staff.IsActive = false;
                int rows = await _db.UpdateAsync(staff);
                return rows > 0;
            }
            return false;
        }
    }
}
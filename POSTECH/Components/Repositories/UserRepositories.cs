using TCZPOS.Components.Database;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TCZPOS.Components.Repositories
{
    public class UserRepositories(DBQueries _db) : IUserRepositories
    {
        private UserModels? _cachedMaster;

        public async Task<List<UserModels>> GetAllUsersAsync()
        {
            return await _db.SelectAllAsync<UserModels>();
        }

        public async Task AddUserAsync(UserModels user)
        {
            await _db.InsertAsync(user);
        }

        public async Task<UserModels?> GetUserByUsernameAsync(string username)
        {
            var results = await _db.SelectFilteredAsync<UserModels>(u =>
                u.Username.ToLower() == username.ToLower());
            return results.FirstOrDefault();
        }

        public async Task UpdateUserAsync(UserModels user)
        {
            await _db.UpdateAsync(user);
        }

        public async Task<bool> RegisterMasterAccountAsync(UserModels user)
        {
            var existing = await GetUserByUsernameAsync(user.Username);

            if (existing != null)
            {
                // Instead of failing, we update the existing record.
                // This handles cases where the user renewed their sub on the web.
                user.Id = existing.Id;
                await _db.UpdateAsync(user);
                return true;
            }

            // If it's a completely new account (different username), allow it!
            int result = await _db.InsertAsync(user);
            return result > 0;
        }
        public async Task<bool> LoginMasterAsync(string username, string password)
        {
            var user = await _db.SelectFilteredAsync<UserModels>(u =>
                u.Username == username && u.IsActive);

            var found = user.FirstOrDefault();

            // Future: Implement BCrypt.Verify(password, found.PasswordHash)
            if (found != null && found.PasswordHash == password)
            {
                _cachedMaster = found;
                return true;
            }
            return false;
        }

        public async Task<UserModels?> GetMasterAccountAsync()
        {
            if (_cachedMaster != null) return _cachedMaster;

            // 2. Look for the ID of the user who actually signed in 
            // (This ID is saved in ProcessLocalLogin)
            var loggedInId = await SecureStorage.Default.GetAsync("MasterId");

            if (!string.IsNullOrEmpty(loggedInId) && int.TryParse(loggedInId, out int userId))
            {
                var results = await _db.SelectFilteredAsync<UserModels>(u => u.Id == userId);
                _cachedMaster = results.FirstOrDefault();

                if (_cachedMaster != null) return _cachedMaster;
            }

            // 3. Fallback: If no session, get the most recently active account
            // This handles the case where the app just opened and no token is set yet
            var allUsers = await _db.SelectAllAsync<UserModels>();
            _cachedMaster = allUsers.OrderByDescending(u => u.LastSyncAt).FirstOrDefault();

            return _cachedMaster;
        }

        public async Task UpdateSubscriptionStatusAsync(bool isSubscribed, DateTime expiry)
        {
            var master = await GetMasterAccountAsync();
            if (master != null)
            {
                master.IsSubscribed = isSubscribed;
                master.SubscriptionExpiry = expiry;
                await _db.UpdateAsync(master);
                _cachedMaster = master;
            }
        }

        public void Logout() => _cachedMaster = null;


    }
}

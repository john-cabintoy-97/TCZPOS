using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IUserRepositories
    {
        Task<List<UserModels>> GetAllUsersAsync();
        Task AddUserAsync(UserModels user);
        Task<UserModels?> GetUserByUsernameAsync(string username);
        Task UpdateUserAsync(UserModels user);


        Task<bool> RegisterMasterAccountAsync(UserModels user);
        Task<bool> LoginMasterAsync(string username, string password);
        Task<UserModels?> GetMasterAccountAsync();
        Task UpdateSubscriptionStatusAsync(bool isSubscribed, DateTime expiry);
        void Logout();
    }
}

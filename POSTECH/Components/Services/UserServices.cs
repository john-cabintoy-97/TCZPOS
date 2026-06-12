using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Services
{
    public class UserServices(IUserRepositories _repo)
    {
        public async Task<bool> IsSubscriptionValidAsync()
        {
            // Dev Bypass for subscription gate

            var master = await _repo.GetMasterAccountAsync();
            if (master == null || !master.IsActive || !master.IsSubscribed) return false;

            if (master.SubscriptionExpiry.HasValue && master.SubscriptionExpiry.Value < DateTime.Now)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            await LogoutAsync();
            // --- DEVELOPMENT BYPASS ---
#if DEBUG
            if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "dev123")
            {
                // 1. Always look for the username actually typed ('alpha')
                var master = await _repo.GetUserByUsernameAsync("admin");

                if (master == null)
                {
                    master = new UserModels
                    {
                        Username = "admin", // Keep this consistent!
                        PasswordHash = "dev123",
                        IsSubscribed = true,
                        SubscriptionExpiry = DateTime.Now.AddYears(1),
                        Role = "Owner",
                        SubscriptionTier = "Free",
                        IsActive = true
                    };
                    await _repo.RegisterMasterAccountAsync(master);

                    // 2. Re-fetch to get the SQLite ID
                    master = await _repo.GetUserByUsernameAsync("admin");
                }

                return await ProcessLocalLogin(master!, "dev123");
            }
#endif
            // 1. TRY LOCAL FIRST (Offline-First)
            var localUser = await _repo.GetUserByUsernameAsync(username);
            if (localUser != null)
            {
                return await ProcessLocalLogin(localUser, password);
            }

            // 2. WEB API FALLBACK (If internet exists)
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {

                //var apiResult = await _apiService.VerifyWebAccount(username, password);

                //if (apiResult.Success)
                //{
                //    var newMaster = new UserModels
                //    {

                //        Username = username,
                //        PasswordHash = password, // Store hash from web
                //        IsSubscribed = apiResult.IsSubscribed,
                //        SubscriptionExpiry = apiResult.ExpiryDate,
                //        Role = apiResult.Role,
                //        CloudUserId = apiResult.UserId
                //    };

                //    await _repo.RegisterMasterAccountAsync(newMaster);

                //    return await ProcessLocalLogin(newMaster, password);
                //}
            }

            return false;
        }

        private async Task<bool> ProcessLocalLogin(UserModels user, string password)
        {
            if (user == null)
            {
                Console.WriteLine("DEBUG: ProcessLocalLogin received a NULL user.");
                return false;
            }

            if (!user.IsActive) return false;

            if (VerifyPassword(password, user.PasswordHash))
            {
                string newToken = Guid.NewGuid().ToString();
                user.LastLoginToken = newToken;

                await _repo.UpdateUserAsync(user);

                // MAUI SecureStorage ensures the session survives app restarts
                await SecureStorage.Default.SetAsync("UserToken", newToken);
                await SecureStorage.Default.SetAsync("IsLoggedIn", "true");
                await SecureStorage.Default.SetAsync("MasterId", user.Id.ToString());

                return true;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("UserToken");
            SecureStorage.Default.Remove("IsLoggedIn");
            SecureStorage.Default.Remove("MasterId");
            _repo.Logout();
        }

        private static bool VerifyPassword(string password, string hash) => password == hash;

        public async Task<List<UserModels>> GetActiveUsers()
        {
            var users = await _repo.GetAllUsersAsync();
            return [..users.Where(u => u.IsActive)];
        }

        public async Task<UserModels?> GetMasterAccountAsync()
        {
            // Simply ask the repository for the currently logged-in Master
            return await _repo.GetMasterAccountAsync();
        }
    }
}
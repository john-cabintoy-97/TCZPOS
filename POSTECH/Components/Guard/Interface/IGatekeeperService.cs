using TCZPOS.Components.Models;

namespace TCZPOS.Components.Guard.Interface
{
    public interface IGatekeeperService
    {
        StaffModels? ActiveStaff { get; }
        UserModels? MasterAccount { get; }
        void InitializeMasterAccount(UserModels master);

        // Updated: Overload for targeted authentication
        Task<bool> AuthenticateStaff(int staffId, string pin);

        // Original: Search by PIN only
        Task<bool> AuthenticateStaff(string pin);

        Task AuthenticateAsOwner(UserModels master);
        void LogoutStaff();
        bool HasPermission(params StaffRole[] requiredRoles);
    }
}
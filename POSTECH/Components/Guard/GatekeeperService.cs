using TCZPOS.Components.Guard.Interface;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;

namespace TCZPOS.Components.Guard
{
    public class GatekeeperService : IGatekeeperService
    {
        private readonly IStaffRepository _repository;
        public StaffModels? ActiveStaff { get; private set; }
        public UserModels? MasterAccount { get; private set; } // Set this after subscription login

        public GatekeeperService(IStaffRepository repository)
        {
            _repository = repository;
        }

        public void InitializeMasterAccount(UserModels master) => MasterAccount = master;

        public async Task<bool> AuthenticateStaff(int staffId, string pin)
        {
            // 1. Check for system initialization
            if (MasterAccount == null) return false;

            // 2. Fetch the specific record
            var staff = await _repository.GetByIdAsync(staffId);
            if (staff == null || !staff.IsActive) return false;

            // 3. Validate permissions and credentials
            if (staff.MasterUserId != MasterAccount.Id) return false;

            if (staff.PinCode != pin) return false;

            // 4. Set the session state
            ActiveStaff = staff;
            ActiveStaff.IsCurrentlyActiveSession = true;

            return true;
        }
        public async Task<bool> AuthenticateStaff(string pin)
        {
            if (MasterAccount == null) return false;

            var staff = await _repository.GetByPinAsync(MasterAccount.Id, pin);
            if (staff != null)
            {
                ActiveStaff = staff;
                ActiveStaff.IsCurrentlyActiveSession = true;
                return true;
            }
            return false;
        }

        public void LogoutStaff()
        {
            if (ActiveStaff != null) ActiveStaff.IsCurrentlyActiveSession = false;
            ActiveStaff = null;
        }

        public bool HasPermission(params StaffRole[] requiredRoles)
        {
            if (ActiveStaff == null) return false;
            // Owners always have permission
            if (ActiveStaff.Role == StaffRole.Owner) return true;

            return requiredRoles.Contains(ActiveStaff.Role);
        }

        public async Task AuthenticateAsOwner(UserModels master)
        {
            ActiveStaff = new StaffModels
            {
                Name = master.Username,
                Role = StaffRole.Owner,
                IsCurrentlyActiveSession = true
            };

            await Task.CompletedTask;
        }
    }
}

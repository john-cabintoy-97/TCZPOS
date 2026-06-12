using TCZPOS.Components.Repositories.Interfaces;
using System.Text;

namespace TCZPOS.Components.Services.Hardware
{
    public class HardwareServices(IHardwareRepositories _hardwareRepo)
    {
        private const string APP_SECRET = "TCZPOS_QPOS_2026_SECURE";
        public bool IsContinuousModeActive { get; set; } = false;
        public async Task VibrateFeedback()
        {
            _hardwareRepo.Vibrate(0.1);
           
        }
        public async Task ProvideScanFeedback()
        {
            _hardwareRepo.Vibrate(0.1);
            await _hardwareRepo.PlayBeep();
        }

        public async Task SoundFeedback()
        {
            await _hardwareRepo.PlayBeep();
        }

        public string GetFormattedConnectionString()
        {
            string ip = _hardwareRepo.GetLocalIPAddress();
            int port = 5000;
            string adminToken = Guid.NewGuid().ToString("N")[..8];

            string timestamp = DateTime.UtcNow.Ticks.ToString();

            string rawData = $"{ip}:{port}:{adminToken}:{timestamp}:{APP_SECRET}";
            string hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawData))[..8];

            return $"TCZPOS_AUTH|{ip}|{port}|{adminToken}|{timestamp}|{hash}";
        }

        public bool IsReadyForConnection()
        {
            return _hardwareRepo.IsNetworkAvailable();
        }

        public async Task<bool> GetCameraPermissionAsync()
        {
            return await _hardwareRepo.RequestCameraPermission();
        }
    }
}

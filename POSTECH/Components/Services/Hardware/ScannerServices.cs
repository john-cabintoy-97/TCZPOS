using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Services.Hardware
{
    public class ScannerServices(HardwareServices _hardwareService)
    {
        public async Task HandleSuccessfulScan()
        {
            await _hardwareService.ProvideScanFeedback();
        }

        public async Task<bool> GetCameraPermissionUI()
        {
            return await _hardwareService.GetCameraPermissionAsync();
        }
    }
}

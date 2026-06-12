using System;
using System.Collections.Generic;
using System.Text;
using TCZPOS.Components.Models;

namespace TCZPOS.Components.Repositories.Interfaces
{
    public interface IHardwareRepositories
    {
        string GetLocalIPAddress();
        bool IsNetworkAvailable();

        string GetDeviceName();
        int GetListenerPort();

        void Vibrate(double durationSeconds = 0.5);
        Task PlayBeep();
        Task<bool> RequestCameraPermission();
        Task PrintReceipt(string storeName, List<SaleDetailModels> items, decimal total);

    }
}

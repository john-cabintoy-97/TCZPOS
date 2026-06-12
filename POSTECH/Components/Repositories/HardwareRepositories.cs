
using Microsoft.Maui.Devices;
using Plugin.Maui.Audio;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TCZPOS.Components.Repositories.Interfaces;
using TCZPOS.Components.Models;
using ESCPOS_NET;
using ESCPOS_NET.Utilities;
using ESCPOS_NET.Printers;
using ESCPOS_NET.Emitters;
#if ANDROID
using Android.App;
using Android.Content;
using Android.Net.Wifi;
#endif

namespace TCZPOS.Components.Repositories
{
    public class HardwareRepositories(IAudioManager _audioManager) : IHardwareRepositories
    {

        public string GetLocalIPAddress()
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var wifiManager = (Android.Net.Wifi.WifiManager)context.GetSystemService(Context.WifiService);

                if (wifiManager != null && wifiManager.ConnectionInfo != null)
                {
                    int ipAddress = wifiManager.ConnectionInfo.IpAddress;
                    return string.Format("{0}.{1}.{2}.{3}",
                        (ipAddress & 0xff),
                        (ipAddress >> 8 & 0xff),
                        (ipAddress >> 16 & 0xff),
                        (ipAddress >> 24 & 0xff));
                }
            }
            catch { }
#else
    try
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                !System.Net.IPAddress.IsLoopback(ip))
            {
                return ip.ToString();
            }
        }
    }
    catch { }
#endif

            return "127.0.0.1"; // Fallback
        }
        public bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public string GetDeviceName()
        {
            return Environment.MachineName;
        }

        public int GetListenerPort()
        {
            return 5000; // You can make this configurable later
        }

        public void Vibrate(double durationSeconds = 0.1)
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                     DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
            }
        }

        public async Task PlayBeep()
        {
            try
            {
                // Place a small 'beep.mp3' in your Resources/Raw folder
                var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("beep.mp3"));
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audio failed: {ex.Message}");
            }
        }

        public async Task<bool> RequestCameraPermission()
        {
            // Use the MainThread to ensure the popup shows up correctly on Android
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }

                return status == PermissionStatus.Granted;
            });
        }

        public async Task PrintReceipt(string storeName, List<SaleDetailModels> items, decimal total)
        {
            try
            {
                string printerIp = "192.168.1.100";
                int port = 9100;

                // Settings object for the constructor
                var settings = new NetworkPrinterSettings()
                {
                    ConnectionString = $"{printerIp}:{port}"
                }; 
                using var printer = new NetworkPrinter(settings);

                // Use the EPSON emitter from the Emitters namespace
                var e = new EPSON();

                var receipt = ByteSplicer.Combine(
                    e.Initialize(),
                    e.CenterAlign(),
                    e.PrintLine(storeName),
                    e.SetStyles(PrintStyle.None), // If PrintStyle is missing, check ESCPOS_NET.Enums
                    e.PrintLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm")),
                    e.PrintLine("--------------------------------"), 
                     e.LeftAlign()
                );

                foreach (var item in items)
                {
                    string line = $"{item.Quantity}x {item.ProductName}".PadRight(22) + item.SubTotal.ToString("N2").PadLeft(10); 
                    receipt = ByteSplicer.Combine(receipt, e.PrintLine(line));
                }

                receipt = ByteSplicer.Combine(
                    receipt,
                    e.PrintLine("--------------------------------"), 
                    e.RightAlign(),
                    e.PrintLine($"TOTAL: {total.ToString("N2")}"),
                    e.PrintLine("\n\n"),
                    e.FeedLines(3),
                    e.FullCut()
                );

                // Version 3.x uses Write()
                printer.Write(receipt);
            }
            catch (Exception ex)
            {
                // For TCZ-POS, you might want to log this to your local database or show a UI alert
                Console.WriteLine($"Printing failed: {ex.Message}");
            }
        }
    }
}

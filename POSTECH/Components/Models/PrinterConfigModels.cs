using SQLite;

namespace TCZPOS.Components.Models
{
    public class PrinterConfigModels
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? ConnectionType { get; set; } // "WiFi", "USB", "Bluetooth"

        // WiFi Fields
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        // USB Fields
        public string? UsbName { get; set; }

        // Bluetooth Fields
        public string? BluetoothMacAddress { get; set; }
    }
}

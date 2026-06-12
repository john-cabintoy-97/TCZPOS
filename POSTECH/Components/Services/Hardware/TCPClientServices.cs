using System.Net.Sockets;
using System.Text;

namespace TCZPOS.Components.Services.Hardware
{
    public class TCPClientServices
    {
        private TcpClient? _persistentClient;
        private NetworkStream? _stream;
        public event Action<string>? OnMessageReceived;

        public static async Task<bool> ConnectViaQR(string qrData)
        {
            try
            {
                var parts = qrData.Split('|');
                if (parts.Length < 5 || parts[0] != "TCZPOS_AUTH") return false;

                string serverIp = parts[1];
                int port = int.Parse(parts[2]);
                string sessionToken = parts[3];

                using TcpClient client = new();
                var connectTask = client.ConnectAsync(serverIp, port);

                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                {
                    throw new Exception("Connection timed out. Check PC Firewall.");
                }

                using NetworkStream stream = client.GetStream();

                ReadOnlyMemory<byte> authData = Encoding.UTF8.GetBytes($"AUTH_TOKEN:{sessionToken}");
                await stream.WriteAsync(authData, CancellationToken.None);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCZPOS Android] Connection Failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StayConnected(string ip, int port)
        {
            try
            {
                _persistentClient = new TcpClient();
                await _persistentClient.ConnectAsync(ip, port);
                _stream = _persistentClient.GetStream();

                _ = Task.Run(ListenForMessages);
                return true;
            }
            catch { return false; }
        }

        private async Task ListenForMessages()
        {
            Memory<byte> buffer = new byte[1024];
            try
            {
                while (_persistentClient?.Connected == true && _stream != null)
                {
                    int read = await _stream.ReadAsync(buffer, CancellationToken.None);
                    if (read == 0) break;

                    var msg = Encoding.UTF8.GetString(buffer.Span[..read]);
                    OnMessageReceived?.Invoke(msg);
                }
            }
            catch { /* Handle disconnect */ }
        }

        public async Task SendLiveMessage(string message)
        {
            if (_stream != null && _persistentClient?.Connected == true)
            {
                ReadOnlyMemory<byte> data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, CancellationToken.None);
            }
        }
    }
}
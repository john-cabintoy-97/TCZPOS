using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCZPOS.Components.Services.Hardware
{
    public class PairingListenerServices
    {
        private TcpListener? _listener;
        private bool _isRunning;
        private const int Port = 5000;

        private readonly List<TcpClient> _connectedClients = [];

        public event Action<string, string>? OnMessageReceived;
        public event Action<int>? OnClientCountChanged;

        public async Task StartListening()
        {
            if (_isRunning) return;
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                _isRunning = true;

                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    lock (_connectedClients)
                    {
                        _connectedClients.Add(client);
                    }
                    OnClientCountChanged?.Invoke(_connectedClients.Count);
                    _ = Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCZPOS] Listener Error: {ex.Message}");
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown Device";
            using var stream = client.GetStream();

            // Using Memory<byte> for the buffer
            Memory<byte> buffer = new byte[1024];

            try
            {
                while (client.Connected && _isRunning)
                {
                    // CHANGE: Using ReadAsync(Memory<byte>, CancellationToken)
                    int read = await stream.ReadAsync(buffer, CancellationToken.None);
                    if (read == 0) break;

                    // Slice the memory to the exact size of data received
                    var msg = Encoding.UTF8.GetString(buffer.Span[..read]);

                    OnMessageReceived?.Invoke(endpoint, msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCZPOS] Connection lost with {endpoint}: {ex.Message}");
            }
            finally
            {
                lock (_connectedClients)
                {
                    _connectedClients.Remove(client);
                }
                OnClientCountChanged?.Invoke(_connectedClients.Count);
                client.Close();
            }
        }

        public async Task BroadcastToAll(string message)
        {
            // Using ReadOnlyMemory<byte> for the data to send
            ReadOnlyMemory<byte> data = Encoding.UTF8.GetBytes(message);
            List<TcpClient> clientsCopy;

            lock (_connectedClients)
            {
                clientsCopy = [.. _connectedClients.Where(c => c.Connected)];
            }

            foreach (var client in clientsCopy)
            {
                try
                {
                    // CHANGE: Using WriteAsync(ReadOnlyMemory<byte>, CancellationToken)
                    await client.GetStream().WriteAsync(data, CancellationToken.None);
                }
                catch
                {
                    // Fail silently; cleanup happens in HandleClient's finally block
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();

            lock (_connectedClients)
            {
                foreach (var client in _connectedClients) client.Close();
                _connectedClients.Clear();
            }
        }

        public int GetConnectedCount() => _connectedClients.Count;
    }
}
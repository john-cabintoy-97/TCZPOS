using TCZPOS.Components.ServiceRegistration;
using SQLite;
namespace TCZPOS.Components.Database
{
    public class ConnectionTemplate : IAsyncDisposable
    {
        private SQLiteAsyncConnection? _connection;
        private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "TCZPOS_v1.db3");
        private readonly SemaphoreSlim _semaphore = new(1, 1); // Prevents multiple services from initializing at once

        public ConnectionTemplate()
        {
            // We don't open the connection in the constructor for SQLite 
            // to avoid blocking the app startup.
        }

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_connection is not null) return _connection;

            await _semaphore.WaitAsync();
            try
            {
                if (_connection is null) // Double-check after acquiring lock
                {
                    _connection = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex);

                    // Ensure tables exist before returning the connection
                    await ServiceTableRegistration.CreateTablesAsync(_connection);
                    Console.WriteLine($"📂 [TCZPOS DEBUG] Database initialized at: {_dbPath}");
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return _connection;
        }
        public string GetDatabasePath() => _dbPath;

        // Clean up when the app closes or the service is disposed
        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection = null;
            }

            // Prevent the finalizer from running, as cleanup is already done.
            GC.SuppressFinalize(this);
        }
    }
}

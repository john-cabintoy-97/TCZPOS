using SQLite;
using System.Linq.Expressions;

namespace TCZPOS.Components.Database
{
    public class DBQueries(ConnectionTemplate _dbTemplate)
    {
        // 1. SELECT (Get All)
        public async Task<List<T>> SelectAllAsync<T>() where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            return await db.Table<T>().ToListAsync();
        }

        // 2. SELECT with Condition (Filter)
        public async Task<List<T>> SelectFilteredAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            return await db.Table<T>().Where(predicate).ToListAsync();
        }

        // 3. INSERT (Dynamic)
        public async Task<int> InsertAsync<T>(T item) where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            // SQLite-Net handles the column mapping automatically
            return await db.InsertAsync(item);
        }

        // 4. UPDATE (Dynamic)
        public async Task<int> UpdateAsync<T>(T item) where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            return await db.UpdateAsync(item);
        }

        // 5. DELETE
        public async Task<int> DeleteAsync<T>(T item) where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            return await db.DeleteAsync(item);
        }

        // 6. TRANSACTION (For Bulk Inventory Updates)
        public async Task ExecuteInTransactionAsync(Action<SQLiteConnection> action)
        {
            var db = await _dbTemplate.GetConnectionAsync();
            // This runs the action inside a transaction and commits if successful
            await db.RunInTransactionAsync(action);
        }

        public async Task<int> RunTransactionAsync(Action<SQLiteConnection> action)
        {
            var db = await _dbTemplate.GetConnectionAsync();
            int rowsAffected = 0;

            await db.RunInTransactionAsync(conn =>
            {
                action(conn);
            });

            return rowsAffected; // Note: SQLite-net's RunInTransaction doesn't return count easily, 
                                 // but we can track it inside the action.
        }

        // 7. MANUAL QUERY (If you need custom SQL)
        public async Task<List<T>> QueryManualAsync<T>(string sql, params object[] args) where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            return await db.QueryAsync<T>(sql, args);
        }

        public async Task<int> ExecuteAsync(string sql, params object[] args)
        {
            var db = await _dbTemplate.GetConnectionAsync();
            // ExecuteAsync returns the number of rows affected
            return await db.ExecuteAsync(sql, args);
        }

        public async Task<T?> GetAsync<T>(object id) where T : new()
        {
            var db = await _dbTemplate.GetConnectionAsync();
            try
            {
                return await db.GetAsync<T>(id);
            }
            catch
            {
                return default; // Returns null if ID not found
            }
        }
    }
}

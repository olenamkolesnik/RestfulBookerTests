using Dapper;
using RestfulBookerTests.Utils;
using Microsoft.Data.SqlClient;

namespace RestfulBookerTests.DB
{
    public class DbClient : IDisposable
    {
        private readonly string _connectionString;

        public DbClient(ConfigManager config)
        {
            _connectionString = config.DbConnectionString;
        }

        public virtual async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
        {
            using var db = new SqlConnection(_connectionString);
            return await db.QuerySingleAsync<T>(sql, parameters);
        }

        public virtual async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync(sql, parameters);
        }

        public virtual void Dispose() { }
    }
}

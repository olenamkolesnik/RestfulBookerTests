using Dapper;
using Microsoft.Data.Sqlite;
using RestfulBookerTests.DB;

namespace RestfulBookerTests.Fakes
{
    public class InMemoryDbClient : IDisposable
    {
        private readonly SqliteConnection _conn;

        public InMemoryDbClient()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            _conn.Execute(@"
                CREATE TABLE Bookings (
                    Firstname TEXT NOT NULL,
                    Lastname TEXT NOT NULL,
                    Totalprice INTEGER,
                    Depositpaid BOOLEAN,
                    Checkin TEXT,
                    Checkout TEXT,
                    Additionalneeds TEXT
                )");

            _conn.Execute(@"
                INSERT INTO Bookings 
                (Firstname, Lastname, Totalprice, Depositpaid, Checkin, Checkout, Additionalneeds)
                VALUES (@Firstname, @Lastname, @Totalprice, @Depositpaid, @Checkin, @Checkout, @Additionalneeds)",
                new
                {
                    Firstname = "John",
                    Lastname = "Doe",
                    Totalprice = 150,
                    Depositpaid = true,
                    Checkin = "2025-08-25",
                    Checkout = "2025-08-30",
                    Additionalneeds = "Breakfast"
                });
        }

        

        public async Task<IEnumerable<TReturn>> QueryMultiMapAsync<TFirst, TSecond, TReturn>(
    string sql,
    Func<TFirst, TSecond, TReturn> map,
    object? parameters = null,
    string splitOn = "Checkin")
        {
            return await _conn.QueryAsync(sql, map, parameters, splitOn: splitOn);
        }


        public  void Dispose()
        {
            _conn.Dispose();
        }
    }
}

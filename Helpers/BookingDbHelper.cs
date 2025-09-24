using RestfulBookerTests.DB;
using RestfulBookerTests.Fakes;
using RestfulBookerTests.Models;

namespace RestfulBookerTests.Helpers
{
    public  class BookingDbHelper
    {
        private readonly InMemoryDbClient _db;

        public BookingDbHelper(InMemoryDbClient db) => _db = db;

        public async Task<Booking> GetBookingAsync(string firstname, string lastname)
        {
            const string sql = @"
                SELECT 
                    Firstname, Lastname, Totalprice, Depositpaid,
                    Additionalneeds, Checkin, Checkout
                FROM Bookings
                WHERE Firstname = @Firstname AND Lastname = @Lastname";

            var result = await _db.QueryMultiMapAsync<Booking, BookingDates, Booking>(
                sql,
                (booking, dates) =>
                {
                    booking.Bookingdates = dates;
                    return booking;
                },
                new { Firstname = firstname, Lastname = lastname },
                splitOn: "Checkin"
            );

            return result.Single();
        }

    }
}

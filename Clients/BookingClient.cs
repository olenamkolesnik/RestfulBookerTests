using Microsoft.Extensions.Logging;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using RestfulBookerTests.Utils;
using RestSharp;
using System.Net;

namespace RestfulBookerTests.Clients
{
    public class BookingClient : BaseClient
    {
        public BookingClient(ConfigManager config, ILogger<BookingClient> logger, LoggingHelper loggingHelper)
            : base(config, logger as ILogger<BaseClient>, loggingHelper)
        {
        }

        public async Task<(BookingCreatedResponse Data, RestResponse Raw, long ElapsedMs)> CreateBookingAsync(Booking booking)
        {
            var request = new RestRequest("/booking", Method.Post).AddJsonBody(booking);
            return await ExecuteAsync<BookingCreatedResponse>(request, "Failed to create booking");
        }

        public async Task<(BookingCreatedResponse Data, RestResponse Raw, long ElapsedMs)> CreateBookingWithoutAuthAsync(Booking booking)
        {
            var request = new RestRequest("/booking", Method.Post).AddJsonBody(booking);
            return await ExecuteAsync<BookingCreatedResponse>(request, "Failed to create booking", requiresAuth: false);
        }

        public async Task<(Booking Data, RestResponse Raw, long ElapsedMs)> GetBookingAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);
            return await ExecuteAsync<Booking>(request, "Failed to retrieve booking");
        }

        public async Task<(Booking Data, RestResponse Raw, long ElapsedMs)> UpdateBookingAsync(int bookingId, Booking booking)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Put).AddJsonBody(booking);
            return await ExecuteAsync<Booking>(request, "Failed to update booking");
        }

        public async Task<(RestResponse Response, long ElapsedMs)> DeleteBookingAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Delete);
            return await ExecuteAsync(request);
        }

        public async Task<(RestResponse Response, long ElapsedMs)> CreateBookingSafeAsync(Booking booking, bool requiresAuth = true)
        {
            var request = new RestRequest("/booking", Method.Post).AddJsonBody(booking);
            return await ExecuteSafeAsync(request, requiresAuth);
        }

        public async Task<(RestResponse Response, long ElapsedMs)> GetBookingSafeAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);
            return await ExecuteSafeAsync(request);
        }

        public async Task<(RestResponse Response, long ElapsedMs)> UpdateBookingSafeAsync(int bookingId, Booking booking)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Put).AddJsonBody(booking);
            return await ExecuteSafeAsync(request);
        }

        public async Task<(RestResponse Response, long ElapsedMs)> DeleteBookingSafeAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Delete);
            return await ExecuteSafeAsync(request);
        }

    }
}

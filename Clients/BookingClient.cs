using Microsoft.Extensions.Logging;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using RestSharp;
using System.Net;

namespace RestfulBookerTests.Clients
{
    public class BookingClient : BaseClient
    {
        public BookingClient(string baseUrl, ILogger logger) : base(baseUrl, logger) { }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        public async Task<(Booking? Booking, HttpStatusCode StatusCode, long ElapsedMs)> GetBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);
            var (response, elapsedMs) = await ExecuteAsync(request, true, cancellationToken);

            if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
            {
                var booking = JsonHelper.DeserializeSafe<Booking>(
                    response.Content,
                    $"Failed to get booking {bookingId}");
                return (booking, response.StatusCode, elapsedMs);
            }

            return (null, response.StatusCode, elapsedMs);
        }

        /// <summary>
        /// Create a booking
        /// </summary>
        public async Task<(BookingCreatedResponse? Result, HttpStatusCode StatusCode, long ElapsedMs)> CreateBookingAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("/booking", Method.Post)
                .AddJsonBody(booking);

            var (response, elapsedMs) = await ExecuteAsync(request, true, cancellationToken);

            if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
            {
                var result = JsonHelper.DeserializeSafe<BookingCreatedResponse>(
                    response.Content,
                    "Failed to create booking");
                return (result, response.StatusCode, elapsedMs);
            }

            return (null, response.StatusCode, elapsedMs);
        }

        /// <summary>
        /// Delete a booking
        /// </summary>
        public async Task<(HttpStatusCode StatusCode, long ElapsedMs)> DeleteBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Delete);
            var (response, elapsedMs) = await ExecuteAsync(request, true, cancellationToken);

            return (response.StatusCode, elapsedMs);
        }
    }
}

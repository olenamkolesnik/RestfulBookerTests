using Microsoft.Extensions.Logging;
using RestfulBookerTests.Models;
using RestSharp;
using System.Net;

namespace RestfulBookerTests.Clients
{
    public class BookingClient : BaseClient
    {
        public BookingClient(string baseUrl, ILogger logger) : base(baseUrl, logger)
        {
        }

        /// <summary>
        /// Creates a new booking.
        /// Returns both the BookingCreatedResponse and the Booking object.
        /// </summary>
        public async Task<(BookingCreatedResponse Result, Booking Booking, HttpStatusCode StatusCode, long ElapsedMs)> CreateBookingAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("/booking", Method.Post)
                .AddJsonBody(booking);

            var (data, raw, elapsedMs) = await ExecuteAsync<BookingCreatedResponse>(
                request,
                "Failed to create booking",
                true,
                cancellationToken
            );

            return (data, data.Booking, raw.StatusCode, elapsedMs);
        }

        /// <summary>
        /// Retrieves a booking by ID.
        /// </summary>
        public async Task<(Booking Booking, HttpStatusCode StatusCode, long ElapsedMs)> GetBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);

            var (data, raw, elapsedMs) = await ExecuteAsync<Booking>(
                request,
                "Failed to retrieve booking",
                true,
                cancellationToken
            );

            return (data, raw.StatusCode, elapsedMs);
        }

        /// <summary>
        /// Updates an existing booking by ID.
        /// Returns the updated Booking object.
        /// </summary>
        public async Task<(Booking UpdatedBooking, HttpStatusCode StatusCode, long ElapsedMs)> UpdateBookingAsync(
            int bookingId,
            Booking updatedBooking,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Put)
                .AddJsonBody(updatedBooking);

            var (data, raw, elapsedMs) = await ExecuteAsync<Booking>(
                request,
                "Failed to update booking",
                true,
                cancellationToken
            );

            return (data, raw.StatusCode, elapsedMs);
        }

        /// <summary>
        /// Deletes a booking by ID.
        /// Returns the HTTP status code and elapsed time.
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

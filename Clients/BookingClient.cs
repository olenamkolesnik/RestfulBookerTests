using Microsoft.Extensions.Logging;
using RestfulBookerTests.Models;
using RestSharp;

namespace RestfulBookerTests.Clients
{
    public class BookingClient : BaseClient
    {
        public BookingClient(string baseUrl, ILogger logger) : base(baseUrl, logger) { }

        /// <summary>
        /// Creates a new booking and returns the booking details and raw response.
        /// </summary>
        public async Task<(BookingCreatedResponse ResponseData, RestResponse RawResponse, long ElapsedMs)> CreateBookingAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("/booking", Method.Post)
                .AddJsonBody(booking);

            var (data, raw, elapsedMs) = await ExecuteSafeAsync<BookingCreatedResponse>(
                request,
                "Failed to create booking.",
                cancellationToken: cancellationToken
            );

            return (data, raw, elapsedMs);
        }

        /// <summary>
        /// Retrieves a booking by its ID.
        /// </summary>
        public async Task<(Booking BookingDetails, RestResponse RawResponse, long ElapsedMs)> GetBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);

            var (data, raw, elapsedMs) = await ExecuteSafeAsync<Booking>(
                request,
                $"Failed to retrieve booking with ID {bookingId}.",
                cancellationToken: cancellationToken
            );

            return (data, raw, elapsedMs);
        }

        /// <summary>
        /// A safe execution wrapper for handling non-2xx responses and deserialization.
        /// </summary>
        private async Task<(T Data, RestResponse Raw, long ElapsedMs)> ExecuteSafeAsync<T>(
            RestRequest request,
            string errorMessageOnDeserialize,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default) where T : class
        {
            var (data, raw, elapsedMs) = await ExecuteAsync<T>(
                request,
                errorMessageOnDeserialize,
                requiresAuth,
                cancellationToken
            );

            // Additional error handling for non-successful responses
            if (!raw.IsSuccessful)
            {
                _logger.LogError("Request to {Url} failed with status {StatusCode}. Response: {Content}",
                    _client.BuildUri(request),
                    raw.StatusCode,
                    raw.Content);

                throw new HttpRequestException($"{errorMessageOnDeserialize} Status: {raw.StatusCode}. Content: {raw.Content}");
            }

            return (data, raw, elapsedMs);
        }
    }
}

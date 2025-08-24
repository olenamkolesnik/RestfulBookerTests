using Microsoft.Extensions.Logging;
using RestfulBookerTests.Models;
using RestSharp;

namespace RestfulBookerTests.Clients
{
    public class BookingClient : BaseClient
    {
        public BookingClient(string baseUrl, ILogger logger) : base(baseUrl, logger) { }

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

        public async Task<(RestResponse Response, long ElapsedMs)> DeleteBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Delete);

            var (response, elapsedMs) = await ExecuteAsync(request, cancellationToken: cancellationToken);

            return (response, elapsedMs);
        }

        public async Task<(Booking BookingDetails, RestResponse RawResponse, long ElapsedMs)> UpdateBookingAsync(
            int bookingId,
            Booking updatedBooking,
            CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Put)
                .AddJsonBody(updatedBooking);

            var (data, raw, elapsedMs) = await ExecuteSafeAsync<Booking>(
                request,
                $"Failed to update booking with ID {bookingId}.",
                cancellationToken: cancellationToken
            );

            return (data, raw, elapsedMs);
        }

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

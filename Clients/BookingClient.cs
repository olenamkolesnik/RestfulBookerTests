using Microsoft.Extensions.Logging;
using RestSharp;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using System.Threading.Tasks;

namespace RestfulBookerTests.Clients
{
    public class BookingClient : BaseClient
    {
        public BookingClient(string baseUrl, ILogger logger) : base(baseUrl, logger)
        {
        }

        public async Task<BookingResponse> CreateBookingAsync(Booking booking)
        {
            var request = new RestRequest("/booking", Method.Post)
                .AddJsonBody(booking);

            var response = await ExecuteAsync<BookingResponse>(request);

            if (!response.IsSuccessful || response.Data is null)
                throw new InvalidOperationException($"Failed to create booking: {response.Content}");

            return response.Data;
        }

        public async Task<Booking> GetBookingAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);

            // Logging before sending request
            LoggingHelper.LogRequest(_logger, Method.Get, request.Resource);

            var client = GetClient();
            var response = await client.ExecuteAsync<Booking>(request);

            // Logging after receiving response
            await LoggingHelper.LogResponseAsync(_logger, response);

            if (!response.IsSuccessful || response.Data is null)
                throw new InvalidOperationException($"Failed to get booking {bookingId}: {response.Content}");

            return response.Data;
        }


        public async Task UpdateBookingAsync(int bookingId, Booking updatedBooking)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Put)
                .AddJsonBody(updatedBooking);

            var response = await ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new InvalidOperationException($"Failed to update booking {bookingId}: {response.Content}");
        }

        public async Task DeleteBookingAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Delete);

            var response = await ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new InvalidOperationException($"Failed to delete booking {bookingId}: {response.Content}");
        }
    }
}

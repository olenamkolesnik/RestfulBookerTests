using Microsoft.Extensions.Logging;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Models;

namespace RestfulBookerTests.Helpers;

public class BookingTestDataHelper
{
    private readonly BookingClient _bookingClient;
    private readonly ILogger<BookingTestDataHelper> _logger;

    public BookingTestDataHelper(BookingClient bookingClient, ILogger<BookingTestDataHelper> logger)
    {
        _bookingClient = bookingClient;
        _logger = logger;
    }

    public async Task<BookingCreatedResponse> CreateBookingAsync(Booking booking)
    {
        var (created, _, elapsedMs) = await _bookingClient.CreateBookingAsync(booking);
        _logger.LogInformation("Created booking ID {BookingId} in {ElapsedMs} ms", created.BookingId, elapsedMs);
        return created;
    }

    public async Task<Booking> GetBookingAsync(int bookingId)
    {
        var (retrieved, _, elapsedMs) = await _bookingClient.GetBookingAsync(bookingId);
        _logger.LogInformation("Retrieved booking ID {BookingId} in {ElapsedMs} ms", bookingId, elapsedMs);
        return retrieved;
    }

    public async Task<BookingCreatedResponse> UpdateBookingAsync(int bookingId, Booking booking)
    {
        var (updated, _, elapsedMs) = await _bookingClient.UpdateBookingAsync(bookingId, booking);
        _logger.LogInformation("Updated booking ID {BookingId} in {ElapsedMs} ms", bookingId, elapsedMs);
        return new BookingCreatedResponse { BookingId = bookingId, Booking = updated };
    }

    public async Task DeleteBookingAsync(int bookingId)
    {
        var (_, elapsedMs) = await _bookingClient.DeleteBookingAsync(bookingId);
        _logger.LogInformation("Deleted booking ID {BookingId} in {ElapsedMs} ms", bookingId, elapsedMs);
    }
}

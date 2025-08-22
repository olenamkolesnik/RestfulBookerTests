using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Models;
using System.Net;

[Binding]
public class BookingSteps(ScenarioContext scenarioContext)
{
    private readonly BookingClient _bookingClient = scenarioContext["BookingClient"] as BookingClient
            ?? throw new InvalidOperationException("BookingClient not found in ScenarioContext");
    private Booking? _booking;
    private BookingCreatedResponse? _createBookingResponse;
    private Booking? _createdBooking;

    [Given("I have a booking with:")]
    public void GivenIHaveABookingWith(Table table)
    {
        if (table.Rows.Count == 0)
            throw new ArgumentException("The booking table must contain at least one row.");

        var row = table.Rows[0];

        _booking = new Booking
        {
            Firstname = row["firstname"],
            Lastname = row["lastname"],
            Totalprice = int.Parse(row["totalprice"]),
            Depositpaid = bool.Parse(row["depositpaid"]),
            Bookingdates = new BookingDates
            {
                Checkin = row["checkin"],
                Checkout = row["checkout"]
            },
            Additionalneeds = row["additionalneeds"]
        };
    }

    [When(@"I send a create booking request")]
    public async Task WhenISendACreateBookingRequest()
    {
        if (_booking == null)
            throw new InvalidOperationException("Booking payload must be initialized before creating booking.");

        (_createBookingResponse, _createdBooking, var _lastStatusCode, var _lastElapsedMs) =
            await _bookingClient.CreateBookingAsync(_booking);

        scenarioContext["LastStatusCode"] = _lastStatusCode;
        scenarioContext["LastElapsedMs"] = _lastElapsedMs;
    }

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        Assert.That(_createBookingResponse?.Bookingid ?? 0, Is.GreaterThan(0), "Booking ID should be greater than zero");
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        if (_createBookingResponse == null)
            throw new InvalidOperationException("Booking creation response is null.");

        var (_getBookingResponse, _lastStatusCode, _lastElapsedMs) = await _bookingClient.GetBookingAsync(_createBookingResponse.Bookingid);

        scenarioContext["LastStatusCode"] = _lastStatusCode;
        scenarioContext["LastElapsedMs"] = _lastElapsedMs;
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatus)
    {
        var _lastStatusCode = scenarioContext.Get<HttpStatusCode>("LastStatusCode");
        var _lastElapsedMs = scenarioContext.Get<long>("LastElapsedMs");
        Assert.That((int)_lastStatusCode, Is.EqualTo(expectedStatus), $"Expected status code {expectedStatus}, but got {(int)_lastStatusCode}");
    }


    [Then(@"the booking details should match what I created")]
    public void ThenTheBookingDetailsShouldMatchWhatICreated()
    {
        if (_booking == null || _createdBooking == null)
            throw new InvalidOperationException("Booking payload or retrieved booking is null.");

        Assert.Multiple(() =>
        {
            Assert.That(_createdBooking.Firstname, Is.EqualTo(_booking.Firstname));
            Assert.That(_createdBooking.Lastname, Is.EqualTo(_booking.Lastname));
            Assert.That(_createdBooking.Totalprice, Is.EqualTo(_booking.Totalprice));
            Assert.That(_createdBooking.Depositpaid, Is.EqualTo(_booking.Depositpaid));
            Assert.That(_createdBooking.Bookingdates.Checkin, Is.EqualTo(_booking.Bookingdates.Checkin));
            Assert.That(_createdBooking.Bookingdates.Checkout, Is.EqualTo(_booking.Bookingdates.Checkout));
            Assert.That(_createdBooking.Additionalneeds, Is.EqualTo(_booking.Additionalneeds));
        });
    }

    [Then(@"the response time should be less than {int} ms")]
    public void ThenTheResponseTimeShouldBeUnder(int maxMilliseconds)
    {
        var _pingElapsedMs = scenarioContext.Get<long>("LastElapsedMs");        

        Assert.That(_pingElapsedMs <= maxMilliseconds, $"Performance test failed: {_pingElapsedMs} ms exceeds {maxMilliseconds} ms");
    }
}

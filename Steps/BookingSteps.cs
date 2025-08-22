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
    private Booking? _bookingUpdated;
    private Booking? _bookingRetrieved;
    private BookingCreatedResponse? _createBookingResponse;
    private Booking? _bookingCreated;

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

        var (responseData, rawResponse, elapsedMs) = await _bookingClient.CreateBookingAsync(_booking);

        _createBookingResponse = responseData;
        _bookingCreated = responseData.Booking;

        scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        scenarioContext["LastElapsedMs"] = elapsedMs;
    }

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        Assert.That(_createBookingResponse?.BookingId ?? 0, Is.GreaterThan(0), "Booking ID should be greater than zero");
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        if (_createBookingResponse == null)
            throw new InvalidOperationException("Booking creation response is null.");

        var (responseData, rawResponse, _lastElapsedMs) = await _bookingClient.GetBookingAsync(_createBookingResponse.BookingId);    

        _bookingRetrieved = responseData;

        scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        scenarioContext["LastElapsedMs"] = _lastElapsedMs;
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatus)
    {
        var lastStatusCode = scenarioContext.Get<HttpStatusCode>("LastStatusCode");
        var lastElapsedMs = scenarioContext.Get<long>("LastElapsedMs");
        Assert.That((int)lastStatusCode, Is.EqualTo(expectedStatus), $"Expected status code {expectedStatus}, but got {(int)lastStatusCode}");
    }


    [Then(@"the booking details should match what I created")]
    public void ThenTheBookingDetailsShouldMatchWhatICreated()
    {
        if (_booking == null || _bookingCreated == null)
            throw new InvalidOperationException("Booking payload or retrieved booking is null.");
        CompareBookings(_booking, _bookingCreated);
    }

    [Then("the booking details should match what I updated")]
    public void ThenTheBookingDetailsShouldMatchWhatIUpdated()
    {
        if (_bookingUpdated == null || _bookingRetrieved == null)
            throw new InvalidOperationException("Booking payload or retrieved booking is null.");
        CompareBookings(_bookingUpdated, _bookingRetrieved);
    }

    private void CompareBookings(Booking actualBooking, Booking expectedBooking)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actualBooking.Firstname, Is.EqualTo(expectedBooking.Firstname));
            Assert.That(actualBooking.Lastname, Is.EqualTo(expectedBooking.Lastname));
            Assert.That(actualBooking.Totalprice, Is.EqualTo(expectedBooking.Totalprice));
            Assert.That(actualBooking.Depositpaid, Is.EqualTo(expectedBooking.Depositpaid));
            Assert.That(actualBooking.Bookingdates.Checkin, Is.EqualTo(expectedBooking.Bookingdates.Checkin));
            Assert.That(actualBooking.Bookingdates.Checkout, Is.EqualTo(expectedBooking.Bookingdates.Checkout));
            Assert.That(actualBooking.Additionalneeds, Is.EqualTo(expectedBooking.Additionalneeds));
        });
    }

    [Then(@"the response time should be less than {int} ms")]
    public void ThenTheResponseTimeShouldBeUnder(int maxMilliseconds)
    {
        var pingElapsedMs = scenarioContext.Get<long>("LastElapsedMs");        

        Assert.That(pingElapsedMs <= maxMilliseconds, $"Performance test failed: {pingElapsedMs} ms exceeds {maxMilliseconds} ms");
    }

    [Given("I have updated booking details with:")]
    public void GivenIHaveUpdatedBookingDetailsWith(Table table)
    {
        if (table.Rows.Count == 0)
            throw new ArgumentException("The booking table must contain at least one row.");

        var row = table.Rows[0];

        _bookingUpdated = new Booking
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

    [When("I send an update booking request for that ID")]
    public async Task WhenISendAnUpdateBookingRequestForThatID()
    {
        if (_bookingUpdated == null)
            throw new InvalidOperationException("BookingUpdated payload must be initialized before creating booking.");
       
        var (responseData, rawResponse, elapsedMs) = await _bookingClient.UpdateBookingAsync(_createBookingResponse!.BookingId, _bookingUpdated);

        scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        scenarioContext["LastElapsedMs"] = elapsedMs;
    }    

    [When("I send a delete booking request for that ID")]
    public async Task WhenISendADeleteBookingRequestForThatID()
    {
        if (_createBookingResponse == null)
            throw new InvalidOperationException("Booking creation response is null.");

        var (rawResponse, _lastElapsedMs) = await _bookingClient.DeleteBookingAsync(_createBookingResponse.BookingId);

        scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        scenarioContext["LastElapsedMs"] = _lastElapsedMs;
    }
}

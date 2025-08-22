using Microsoft.Extensions.Logging;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Models;

[Binding]
public class BookingSteps
{
    private readonly BookingClient _bookingClient;
    private readonly ILogger _logger;
    private Booking? _bookingPayload;
    private BookingResponse? _createBookingResponse;
    private Booking? _getBookingResponse;

    public BookingSteps(ScenarioContext scenarioContext)
    {
        _bookingClient = scenarioContext["BookingClient"] as BookingClient
            ?? throw new InvalidOperationException("BookingClient not found in ScenarioContext");
        _logger = scenarioContext["Logger"] as ILogger
            ?? throw new InvalidOperationException("Logger not found in ScenarioContext");
    }

    [Given(@"I have valid booking details")]
    public void GivenIHaveValidBookingDetails()
    {
        _bookingPayload = new Booking
        {
            Firstname = "John",
            Lastname = "Doe",
            Totalprice = 150,
            Depositpaid = true,
            Bookingdates = new BookingDates
            {
                Checkin = "2025-09-01",
                Checkout = "2025-09-10"
            },
            Additionalneeds = "Breakfast"
        };
    }

    [When(@"I send a create booking request")]
    public async Task WhenISendACreateBookingRequest()
    {
        if (_bookingPayload == null)
            throw new InvalidOperationException("Booking payload must be initialized before creating booking.");

      //  _createBookingResponse = await _bookingClient.CreateBookingAsync(_bookingPayload);
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        if (_createBookingResponse == null)
            throw new InvalidOperationException("Booking creation response is null.");

      //  _getBookingResponse = await _bookingClient.GetBookingAsync(_createBookingResponse.BookingId);
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatus)
    {
        // You can check both creation and retrieval
        int createStatus = _createBookingResponse != null ? 200 : 0; // RestSharp returns 200 if success
        int getStatus = _getBookingResponse != null ? 200 : 0;

        Assert.That(createStatus, Is.EqualTo(expectedStatus), "Unexpected creation status");
        Assert.That(getStatus, Is.EqualTo(expectedStatus), "Unexpected retrieval status");
    }

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        Assert.That(_createBookingResponse?.BookingId ?? 0, Is.GreaterThan(0), "Booking ID should be greater than zero");
    }

    [Then(@"the booking details should match what I created")]
    public void ThenTheBookingDetailsShouldMatchWhatICreated()
    {
        if (_bookingPayload == null || _getBookingResponse == null)
            throw new InvalidOperationException("Booking payload or retrieved booking is null.");

        Assert.Multiple(() =>
        {
            Assert.That(_getBookingResponse.Firstname, Is.EqualTo(_bookingPayload.Firstname));
            Assert.That(_getBookingResponse.Lastname, Is.EqualTo(_bookingPayload.Lastname));
            Assert.That(_getBookingResponse.Totalprice, Is.EqualTo(_bookingPayload.Totalprice));
            Assert.That(_getBookingResponse.Depositpaid, Is.EqualTo(_bookingPayload.Depositpaid));
            Assert.That(_getBookingResponse.Bookingdates.Checkin, Is.EqualTo(_bookingPayload.Bookingdates.Checkin));
            Assert.That(_getBookingResponse.Bookingdates.Checkout, Is.EqualTo(_bookingPayload.Bookingdates.Checkout));
            Assert.That(_getBookingResponse.Additionalneeds, Is.EqualTo(_bookingPayload.Additionalneeds));
        });
    }
}

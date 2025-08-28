using Microsoft.Extensions.Logging;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using System.Net;

[Binding]
public class BookingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly BookingClient _bookingClient;
    private readonly ILogger<BookingSteps> _logger;

    public BookingSteps(
        ScenarioContext scenarioContext,
        BookingClient bookingClient,
        ILogger<BookingSteps> logger)
    {
        _scenarioContext = scenarioContext;
        _bookingClient = bookingClient;
        _logger = logger;
    }

    #region Given Steps

    [Given("I have a booking with:")]
    public void GivenIHaveABookingWith(Table table)
    {
        var booking = ParseBookingTable(table);
        _scenarioContext.SetData(ScenarioKeys.CurrentBooking, booking);
    }

    [Given("I have updated booking details with:")]
    public void GivenIHaveUpdatedBookingDetailsWith(Table table)
    {
        var booking = ParseBookingTable(table);
        _scenarioContext.SetData(ScenarioKeys.UpdatedBooking, booking);
    }

    #endregion

    #region When Steps (Happy Paths)

    [When("I send a create booking request")]
    public async Task WhenISendACreateBookingRequest()
    {
        var booking = _scenarioContext.GetData<Booking>(ScenarioKeys.CurrentBooking);
        var (data, raw, elapsedMs) = await _bookingClient.CreateBookingAsync(booking);

        _scenarioContext.SetData(ScenarioKeys.BookingCreatedResponse, data);
        _scenarioContext.SetData(ScenarioKeys.CreatedBooking, data.Booking);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, raw.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        var (data, raw, elapsedMs) = await _bookingClient.GetBookingAsync(createResponse.BookingId);

        _scenarioContext.SetData(ScenarioKeys.RetrievedBooking, data);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, raw.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When("I send an update booking request for that ID")]
    public async Task WhenISendAnUpdateBookingRequestForThatID()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        var updatedBooking = _scenarioContext.GetData<Booking>(ScenarioKeys.UpdatedBooking);

        var (updatedData, raw, elapsedMs) = await _bookingClient.UpdateBookingAsync(createResponse.BookingId, updatedBooking);

        _scenarioContext.SetData(ScenarioKeys.BookingCreatedResponse, new BookingCreatedResponse
        {
            BookingId = createResponse.BookingId,
            Booking = updatedData
        });
        _scenarioContext.SetData(ScenarioKeys.RetrievedBooking, updatedData);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, raw.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When("I send a delete booking request for that ID")]
    public async Task WhenISendADeleteBookingRequestForThatID()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        var (response, elapsedMs) = await _bookingClient.DeleteBookingAsync(createResponse.BookingId);

        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, response.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    #endregion

    #region When Steps (Negative & Edge Cases)

    [When("I send a create booking request without auth")]
    public async Task WhenISendACreateBookingRequestWithoutAuth()
    {
        var booking = _scenarioContext.GetData<Booking>(ScenarioKeys.CurrentBooking);
        var (response, elapsedMs) = await _bookingClient.CreateBookingSafeAsync(booking,requiresAuth:false);

        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, response.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When(@"I send a get booking request for ID (.*)")]
    public async Task WhenISendAGetBookingRequestForId(int bookingId)
    {
        var (response, elapsedMs) = await _bookingClient.GetBookingSafeAsync(bookingId);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, response.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When(@"I send an update booking request for ID (.*)")]
    public async Task WhenISendAnUpdateBookingRequestForId(int bookingId)
    {
        var updatedBooking = _scenarioContext.GetData<Booking>(ScenarioKeys.UpdatedBooking);
        var (response, elapsedMs) = await _bookingClient.UpdateBookingSafeAsync(bookingId, updatedBooking);

        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, response.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When(@"I send a delete booking request for ID (.*)")]
    public async Task WhenISendADeleteBookingRequestForId(int bookingId)
    {
        var (response, elapsedMs) = await _bookingClient.DeleteBookingSafeAsync(bookingId);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, response.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    #endregion

    #region Then Steps

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        Assert.That(createResponse.BookingId, Is.GreaterThan(0));
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatus)
    {
        var lastStatusCode = _scenarioContext.GetData<HttpStatusCode>(ScenarioKeys.LastStatusCode);
        AssertionHelper.AssertStatusCode(lastStatusCode, expectedStatus);
    }

    [Then(@"the booking details should match what I created")]
    public void ThenTheBookingDetailsShouldMatchWhatICreated()
    {
        var expected = _scenarioContext.GetData<Booking>(ScenarioKeys.CurrentBooking);
        var actual = _scenarioContext.GetData<Booking>(ScenarioKeys.CreatedBooking);
        AssertionHelper.AssertBookingEquality(expected, actual);
    }

    [Then(@"the booking details should match what I updated")]
    public void ThenTheBookingDetailsShouldMatchWhatIUpdated()
    {
        var expected = _scenarioContext.GetData<Booking>(ScenarioKeys.UpdatedBooking);
        var actual = _scenarioContext.GetData<Booking>(ScenarioKeys.RetrievedBooking);
        AssertionHelper.AssertBookingEquality(expected, actual);
    }

    [Then(@"the response matches the ""(.*)"" schema")]
    public void ThenTheResponseMatchesSchema(string schemaName)
    {
        var response = _scenarioContext.GetData<object>(schemaName);
        Assert.That(response, Is.Not.Null, $"Response for schema '{schemaName}' is null.");
        SchemaValidationHelper.ValidateAgainstSchema(response);
        _logger.LogInformation("Schema validation passed for '{SchemaName}'.", schemaName);
    }

    [Then(@"the response time should be less than (.*) ms")]
    public void ThenTheResponseTimeShouldBeUnder(int maxMilliseconds)
    {
        var elapsedMs = _scenarioContext.GetData<long>(ScenarioKeys.LastElapsedMs);
        AssertionHelper.AssertResponseTime(elapsedMs, maxMilliseconds);
    }

    #endregion

    #region Helpers

    private static Booking ParseBookingTable(Table table)
    {
        var row = table.Rows[0];
        return new Booking
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

    #endregion
}

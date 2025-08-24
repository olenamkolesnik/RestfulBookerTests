using Json.Schema;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Models;
using RestSharp;
using System.Net;
using System.Reflection;
using System.Text.Json;

[Binding]
public class BookingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly BookingClient _bookingClient;

    public BookingSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _bookingClient = _scenarioContext["BookingClient"] as BookingClient
                         ?? throw new InvalidOperationException("BookingClient not found in ScenarioContext");
    }

    [Given("I have a booking with:")]
    public void GivenIHaveABookingWith(Table table)
    {
        var booking = ParseBookingTable(table);
        _scenarioContext["CurrentBooking"] = booking;
    }

    [Given("I have updated booking details with:")]
    public void GivenIHaveUpdatedBookingDetailsWith(Table table)
    {
        var booking = ParseBookingTable(table);
        _scenarioContext["UpdatedBooking"] = booking;
    }

    [When(@"I send a create booking request")]
    public async Task WhenISendACreateBookingRequest()
    {
        Booking booking = _scenarioContext.Get<Booking>("CurrentBooking");

        (BookingCreatedResponse responseData, RestResponse rawResponse, long elapsedMs) =
            await _bookingClient.CreateBookingAsync(booking);

        _scenarioContext["BookingCreatedResponse"] = responseData;
        _scenarioContext["CreatedBooking"] = responseData.Booking;
        _scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        _scenarioContext["LastElapsedMs"] = elapsedMs;
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        BookingCreatedResponse createResponse = _scenarioContext.Get<BookingCreatedResponse>("BookingCreatedResponse");

        (Booking retrievedBooking, RestResponse rawResponse, long elapsedMs) =
            await _bookingClient.GetBookingAsync(createResponse.BookingId);

        _scenarioContext["RetrievedBooking"] = retrievedBooking;
        _scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        _scenarioContext["LastElapsedMs"] = elapsedMs;
    }

    [When("I send an update booking request for that ID")]
    public async Task WhenISendAnUpdateBookingRequestForThatID()
    {
        BookingCreatedResponse createResponse = _scenarioContext.Get<BookingCreatedResponse>("BookingCreatedResponse");
        Booking updatedBooking = _scenarioContext.Get<Booking>("UpdatedBooking");

        (Booking updatedData, RestResponse rawResponse, long elapsedMs) =
            await _bookingClient.UpdateBookingAsync(createResponse.BookingId, updatedBooking);

        var wrappedResponse = new BookingCreatedResponse
        {
            BookingId = createResponse.BookingId,
            Booking = updatedData
        };

        _scenarioContext["BookingCreatedResponse"] = wrappedResponse;
        _scenarioContext["RetrievedBooking"] = updatedData;
        _scenarioContext["LastStatusCode"] = rawResponse.StatusCode;
        _scenarioContext["LastElapsedMs"] = elapsedMs;
    }

    [When("I send a delete booking request for that ID")]
    public async Task WhenISendADeleteBookingRequestForThatID()
    {
        BookingCreatedResponse createResponse = _scenarioContext.Get<BookingCreatedResponse>("BookingCreatedResponse");

        (RestResponse response, long elapsedMs) = await _bookingClient.DeleteBookingAsync(createResponse.BookingId);

        _scenarioContext["LastStatusCode"] = response.StatusCode;
        _scenarioContext["LastElapsedMs"] = elapsedMs;
    }

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        BookingCreatedResponse createResponse = _scenarioContext.Get<BookingCreatedResponse>("BookingCreatedResponse");
        Assert.That(createResponse.BookingId, Is.GreaterThan(0));
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatus)
    {
        HttpStatusCode lastStatusCode = _scenarioContext.Get<HttpStatusCode>("LastStatusCode");
        Assert.That((int)lastStatusCode, Is.EqualTo(expectedStatus), $"Expected status code was {expectedStatus}, but was {(int)lastStatusCode}");
    }

    [Then(@"the booking details should match what I created")]
    public void ThenTheBookingDetailsShouldMatchWhatICreated()
    {
        Booking expected = _scenarioContext.Get<Booking>("CurrentBooking");
        Booking actual = _scenarioContext.Get<Booking>("CreatedBooking");

        CompareBookings(expected, actual);
    }

    [Then("the booking details should match what I updated")]
    public void ThenTheBookingDetailsShouldMatchWhatIUpdated()
    {
        Booking expected = _scenarioContext.Get<Booking>("UpdatedBooking");
        Booking actual = _scenarioContext.Get<Booking>("RetrievedBooking");

        CompareBookings(expected, actual);
    }

    [Then(@"the response time should be less than (.*) ms")]
    public void ThenTheResponseTimeShouldBeUnder(int maxMilliseconds)
    {
        long elapsedMs = _scenarioContext.Get<long>("LastElapsedMs");
        Assert.That(elapsedMs, Is.LessThanOrEqualTo(maxMilliseconds));
    }

    [Then(@"the response matches the ""(.*)"" schema")]
    public void ThenTheResponseMatchesSchema(string schemaName)
    {
        bool hasResponse = _scenarioContext.TryGetValue<object>(schemaName, out object lastResponseBody);
        Assert.That(hasResponse && lastResponseBody != null);

        var modelType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase));
        Assert.That(modelType, Is.Not.Null);

        string schemaJson = SchemaGeneratorHelper.GenerateSchemaAsString(modelType!);
        var schema = JsonSerializer.Deserialize<JsonSchema>(schemaJson);
        Assert.That(schema, Is.Not.Null);

        string jsonString = JsonSerializer.Serialize(lastResponseBody);
        using var jsonDoc = JsonDocument.Parse(jsonString);

        var result = schema!.Evaluate(jsonDoc.RootElement);
        Assert.That(result.IsValid, Is.True, $"Schema validation failed for '{schemaName}'");
    }

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

    private static void CompareBookings(Booking expected, Booking actual)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.Firstname, Is.EqualTo(expected.Firstname));
            Assert.That(actual.Lastname, Is.EqualTo(expected.Lastname));
            Assert.That(actual.Totalprice, Is.EqualTo(expected.Totalprice));
            Assert.That(actual.Depositpaid, Is.EqualTo(expected.Depositpaid));
            Assert.That(actual.Bookingdates.Checkin, Is.EqualTo(expected.Bookingdates.Checkin));
            Assert.That(actual.Bookingdates.Checkout, Is.EqualTo(expected.Bookingdates.Checkout));
            Assert.That(actual.Additionalneeds, Is.EqualTo(expected.Additionalneeds));
        });
    }
}

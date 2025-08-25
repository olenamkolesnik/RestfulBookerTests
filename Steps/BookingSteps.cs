using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Extensions;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Json.Schema;

[Binding]
public class BookingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly BookingClient _bookingClient;

    public BookingSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _bookingClient = _scenarioContext.GetClient<BookingClient>();
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

    #region When Steps

    [When(@"I send a create booking request")]
    public async Task WhenISendACreateBookingRequest()
    {
        var booking = _scenarioContext.GetData<Booking>(ScenarioKeys.CurrentBooking);
        Assert.That(booking, Is.Not.Null, "Current booking payload is null.");

        var (responseData, rawResponse, elapsedMs) = await _bookingClient.CreateBookingAsync(booking);

        _scenarioContext.SetData(ScenarioKeys.BookingCreatedResponse, responseData);
        _scenarioContext.SetData(ScenarioKeys.CreatedBooking, responseData.Booking);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, rawResponse.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");

        var (retrievedBooking, rawResponse, elapsedMs) = await _bookingClient.GetBookingAsync(createResponse.BookingId);

        _scenarioContext.SetData(ScenarioKeys.RetrievedBooking, retrievedBooking);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, rawResponse.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When("I send an update booking request for that ID")]
    public async Task WhenISendAnUpdateBookingRequestForThatID()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        var updatedBooking = _scenarioContext.GetData<Booking>(ScenarioKeys.UpdatedBooking);

        Assert.Multiple(() =>
        {
            Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");
            Assert.That(updatedBooking, Is.Not.Null, "Updated booking payload is null.");
        });

        var (updatedData, rawResponse, elapsedMs) = await _bookingClient.UpdateBookingAsync(createResponse.BookingId, updatedBooking);

        var wrappedResponse = new BookingCreatedResponse
        {
            BookingId = createResponse.BookingId,
            Booking = updatedData
        };

        _scenarioContext.SetData(ScenarioKeys.BookingCreatedResponse, wrappedResponse);
        _scenarioContext.SetData(ScenarioKeys.RetrievedBooking, updatedData);
        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, rawResponse.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    [When("I send a delete booking request for that ID")]
    public async Task WhenISendADeleteBookingRequestForThatID()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);
        Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");

        var (response, elapsedMs) = await _bookingClient.DeleteBookingAsync(createResponse.BookingId);

        _scenarioContext.SetData(ScenarioKeys.LastStatusCode, response.StatusCode);
        _scenarioContext.SetData(ScenarioKeys.LastElapsedMs, elapsedMs);
    }

    #endregion

    #region Then Steps

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        var createResponse = _scenarioContext.GetData<BookingCreatedResponse>(ScenarioKeys.BookingCreatedResponse);

        Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");
        Assert.That(createResponse.BookingId, Is.GreaterThan(0), "Booking ID should be greater than zero");
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
        if (!_scenarioContext.TryGetData<object>(schemaName, out var lastResponseBody) || lastResponseBody == null)
        {
            Assert.Fail($"Response body for '{schemaName}' not found in ScenarioContext.");
        }

        var modelType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase));

        Assert.That(modelType, Is.Not.Null, $"Type '{schemaName}' not found in current assembly.");

        string schemaJson = SchemaGeneratorHelper.GenerateSchemaAsString(modelType!);
        var schema = JsonSerializer.Deserialize<JsonSchema>(schemaJson);
        Assert.That(schema, Is.Not.Null, "Schema deserialization failed.");

        var jsonString = JsonSerializer.Serialize(lastResponseBody);
        using var jsonDoc = JsonDocument.Parse(jsonString);

        var result = schema!.Evaluate(jsonDoc.RootElement);

        Assert.That(result.IsValid, Is.True,
            $"Schema validation failed for '{schemaName}':\n" +
            string.Join("\n", result.Details
                .Where(d => d.HasErrors)
                .SelectMany(d => d.Errors!.Select(e => $"{d.InstanceLocation}: {e.Key} - {e.Value}"))));
    }

    [Then(@"the response time should be less than {int} ms")]
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

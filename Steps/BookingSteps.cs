using Json.Schema;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Extensions;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Models;
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
        // Use typed extension method instead of string key
        _bookingClient = _scenarioContext.GetBookingClient();
    }

    [Given("I have a booking with:")]
    public void GivenIHaveABookingWith(Table table)
    {
        var booking = ParseBookingTable(table);
        _scenarioContext.SetCurrentBooking(booking);
    }

    [Given("I have updated booking details with:")]
    public void GivenIHaveUpdatedBookingDetailsWith(Table table)
    {
        var booking = ParseBookingTable(table);
        _scenarioContext.SetUpdatedBooking(booking);
    }

    [When(@"I send a create booking request")]
    public async Task WhenISendACreateBookingRequest()
    {
        var booking = _scenarioContext.GetCurrentBooking();
        Assert.That(booking, Is.Not.Null, "Current booking payload is null.");

        var (responseData, rawResponse, elapsedMs) = await _bookingClient.CreateBookingAsync(booking);

        _scenarioContext.SetBookingCreatedResponse(responseData);
        _scenarioContext.SetCreatedBooking(responseData.Booking);
        _scenarioContext.SetLastStatusCode(rawResponse.StatusCode);
        _scenarioContext.SetLastElapsedMs(elapsedMs);
    }

    [When(@"I send a get booking request for that ID")]
    public async Task WhenISendAGetBookingRequestForThatId()
    {
        var createResponse = _scenarioContext.GetBookingCreatedResponse();
        Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");

        var (retrievedBooking, rawResponse, elapsedMs) = await _bookingClient.GetBookingAsync(createResponse.BookingId);

        _scenarioContext.SetRetrievedBooking(retrievedBooking);
        _scenarioContext.SetLastStatusCode(rawResponse.StatusCode);
        _scenarioContext.SetLastElapsedMs(elapsedMs);
    }

    [When("I send an update booking request for that ID")]
    public async Task WhenISendAnUpdateBookingRequestForThatID()
    {
        var createResponse = _scenarioContext.GetBookingCreatedResponse();
        var updatedBooking = _scenarioContext.GetUpdatedBooking();

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

        _scenarioContext.SetBookingCreatedResponse(wrappedResponse);
        _scenarioContext.SetRetrievedBooking(updatedData);
        _scenarioContext.SetLastStatusCode(rawResponse.StatusCode);
        _scenarioContext.SetLastElapsedMs(elapsedMs);
    }

    [When("I send a delete booking request for that ID")]
    public async Task WhenISendADeleteBookingRequestForThatID()
    {
        var createResponse = _scenarioContext.GetBookingCreatedResponse();
        Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");

        var (response, elapsedMs) = await _bookingClient.DeleteBookingAsync(createResponse.BookingId);

        _scenarioContext.SetLastStatusCode(response.StatusCode);
        _scenarioContext.SetLastElapsedMs(elapsedMs);
    }

    [Then(@"the booking ID should be returned")]
    public void ThenTheBookingIdShouldBeReturned()
    {
        var createResponse = _scenarioContext.GetBookingCreatedResponse();
        Assert.That(createResponse, Is.Not.Null, "Booking creation response is null.");
        Assert.That(createResponse.BookingId, Is.GreaterThan(0), "Booking ID should be greater than zero");
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatus)
    {
        var lastStatusCode = _scenarioContext.GetLastStatusCode();
        AssertionHelper.AssertStatusCode(lastStatusCode, expectedStatus);
    }

    [Then(@"the booking details should match what I created")]
    public void ThenTheBookingDetailsShouldMatchWhatICreated()
    {
        var expected = _scenarioContext.GetCurrentBooking();
        var actual = _scenarioContext.GetCreatedBooking();

        AssertionHelper.AssertBookingEquality(expected, actual);
    }

    [Then(@"the booking details should match what I updated")]
    public void ThenTheBookingDetailsShouldMatchWhatIUpdated()
    {
        var expected = _scenarioContext.GetUpdatedBooking();
        var actual = _scenarioContext.GetRetrievedBooking();

        AssertionHelper.AssertBookingEquality(expected, actual);
    }

    [Then(@"the response matches the ""(.*)"" schema")]
    public void ThenTheResponseMatchesSchema(string schemaName)
    {
        // This method might need adjustment based on how you want to handle schema validation
        // For now, keeping the original logic but you could create typed extensions for this too
        var hasResponse = _scenarioContext.TryGetValue<object>(schemaName, out var lastResponseBody);
        Assert.That(hasResponse && lastResponseBody != null);

        var modelType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name.Equals(schemaName, StringComparison.OrdinalIgnoreCase));
        Assert.That(modelType, Is.Not.Null);

        string schemaJson = SchemaGeneratorHelper.GenerateSchemaAsString(modelType!);
        var schema = JsonSerializer.Deserialize<JsonSchema>(schemaJson);
        Assert.That(schema, Is.Not.Null);

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
        var elapsedMs = _scenarioContext.GetLastElapsedMs();
        AssertionHelper.AssertResponseTime(elapsedMs, maxMilliseconds);
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
}
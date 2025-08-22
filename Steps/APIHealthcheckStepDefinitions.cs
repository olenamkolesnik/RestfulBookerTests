using Microsoft.Extensions.Logging;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Utils;
using RestSharp;
using System.Net;

namespace RestfulBookerTests.Steps
{
    [Binding]
    public class HealthCheckSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ILogger _logger;
        private readonly HealthCheckClient _healthCheckClient;

        private RestResponse _pingResponse;
        private long _pingElapsedMs;

        public HealthCheckSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _logger = (ILogger)_scenarioContext["Logger"];
            _healthCheckClient = _scenarioContext.Get<HealthCheckClient>("HealthCheckClient");
        }

        [When(@"I send a ping request")]
        public async Task WhenISendAPingRequest()
        {
            var healthClient = _scenarioContext.Get<HealthCheckClient>("HealthCheckClient");
            (_pingResponse, _pingElapsedMs) = await healthClient.PingAsync();
       
            _logger.LogInformation("Ping returned {StatusCode} with content: {Content} in {ElapsedMs} ms", _pingResponse.StatusCode, _pingResponse.Content, _pingElapsedMs);
        }

        [Then(@"the response status code should be {int}")]
        public void ThenTheResponseShouldBe200OK(int statusCode)
        {
            HttpStatusCode expectedHttpStatusCode = (HttpStatusCode)statusCode;
            Assert.That(_pingResponse.StatusCode, Is.EqualTo(expectedHttpStatusCode), $"Expected {expectedHttpStatusCode}, but got {_pingResponse.StatusCode}");

            _logger.LogInformation("Ping returned {ExpectedStatusCode} with content: {Content}", expectedHttpStatusCode, _pingResponse);
        }

        [Then(@"the response time should be less than {int} ms")]
        public void ThenTheResponseTimeShouldBeUnder(int maxMilliseconds)
        {
            _logger.LogInformation("Ping response time: {Elapsed} ms (threshold: {Max} ms)", _pingElapsedMs, maxMilliseconds);
            Assert.That(_pingElapsedMs <= maxMilliseconds, $"Performance test failed: {_pingElapsedMs} ms exceeds {maxMilliseconds} ms");
        }
    }
}

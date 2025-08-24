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

        public HealthCheckSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _logger = (ILogger)_scenarioContext["Logger"];
            _healthCheckClient = _scenarioContext.Get<HealthCheckClient>("HealthCheckClient");
        }

        [When(@"I send a ping request")]
        public async Task WhenISendAPingRequest()
        {/*
            var (_pingResponse, _pingElapsedMs) = await _healthCheckClient.PingAsync();

            _scenarioContext["LastStatusCode"] = _pingResponse.StatusCode;
            _scenarioContext["LastElapsedMs"] = _pingElapsedMs;*/
        }

       
    }
}

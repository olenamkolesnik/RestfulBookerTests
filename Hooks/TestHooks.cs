using Microsoft.Extensions.Logging;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Utils;

namespace RestfulBookerTests.Hooks
{
    [Binding]
    public class TestHooks
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ILoggerFactory _loggerFactory;

        public BaseClient BaseClient { get; private set; }
        public BookingClient BookingClient { get; private set; }
        public ILogger Logger { get; private set; }

        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;

            // Logger setup using LoggerFactory.Create
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                if (Enum.TryParse<LogLevel>(ConfigManager.LogLevel, true, out var logLevel))
                    builder.SetMinimumLevel(logLevel);
                else
                    builder.SetMinimumLevel(LogLevel.Debug); // fallback

                builder.AddConsole();
            });

            Logger = _loggerFactory.CreateLogger("TestLogger");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            // Initialize clients per scenario (parallel-safe)
            BaseClient = new BaseClient(ConfigManager.BaseUrl, Logger);
            BookingClient = new BookingClient(ConfigManager.BaseUrl, Logger);

            // Authenticate BaseClient and share token
            var token = BaseClient.AuthenticateAsync(ConfigManager.Username, ConfigManager.Password)
                                  .GetAwaiter().GetResult();
            BaseClient.SetToken(token);
            BookingClient.SetToken(token);

            // Store in ScenarioContext for step definitions
            _scenarioContext["BaseClient"] = BaseClient;
            _scenarioContext["BookingClient"] = BookingClient;
            _scenarioContext["Logger"] = Logger;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            // Dispose clients if they implement IDisposable
            (BaseClient as IDisposable)?.Dispose();
            (BookingClient as IDisposable)?.Dispose();

            // Dispose logger factory to release resources
            _loggerFactory.Dispose();
        }
    }
}

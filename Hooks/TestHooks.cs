using Microsoft.Extensions.Logging;
using Reqnroll;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Extensions;
using RestfulBookerTests.Utils;

namespace RestfulBookerTests.Hooks
{
    [Binding]
    public class TestHooks
    {
        private readonly ScenarioContext _scenarioContext;

        public BaseClient BaseClient { get; private set; }
        public BookingClient BookingClient { get; private set; }
        public ILogger Logger { get; private set; }

        private static ILoggerFactory _loggerFactory;

        static TestHooks()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(ConfigManager.LogLevel);
                builder.AddConsole();
            });
        }

        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            Logger = _loggerFactory.CreateLogger("TestLogger");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            // Initialize clients per scenario (parallel-safe)
            BaseClient = new BaseClient(ConfigManager.BaseUrl, Logger);
            BookingClient = new BookingClient(ConfigManager.BaseUrl, Logger);

            // Store clients using typed extension methods
            _scenarioContext.SetClient(BaseClient);
            _scenarioContext.SetClient(BookingClient);
            _scenarioContext.SetLogger(Logger);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            (BaseClient as IDisposable)?.Dispose();
            (BookingClient as IDisposable)?.Dispose();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _loggerFactory.Dispose();
        }
    }
}
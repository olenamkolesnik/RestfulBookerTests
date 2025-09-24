using Reqnroll;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Configuration;
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Utils;

[Binding]
public sealed class TestHooks
{
    private readonly IObjectContainer _container;

    public TestHooks(IObjectContainer container)
    {
        _container = container;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        DotNetEnv.Env.Load();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        // ----- Build configuration -----
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        _container.RegisterInstanceAs<IConfiguration>(configuration);
        _container.RegisterTypeAs<ConfigManager, ConfigManager>();        // singleton by default
        _container.RegisterTypeAs<LoggingHelper, LoggingHelper>();

        // ----- Logging -----
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            var logLevel = configuration.GetValue("Api:LogLevel", "Debug");
            if (!Enum.TryParse<LogLevel>(logLevel, true, out var level))
                level = LogLevel.Debug;

            builder.SetMinimumLevel(level);
            builder.AddConsole();
        });

        // Register factory and typed loggers you need
        _container.RegisterInstanceAs(loggerFactory);
        _container.RegisterInstanceAs(loggerFactory.CreateLogger<BookingSteps>());
        _container.RegisterInstanceAs(loggerFactory.CreateLogger<BookingClient>());
        _container.RegisterInstanceAs(loggerFactory.CreateLogger<BaseClient>());


        // ----- Clients (scoped per scenario) -----
        _container.RegisterTypeAs<BaseClient, BaseClient>();
        _container.RegisterTypeAs<BookingClient, BookingClient>();

        // ----- Test helpers -----
        _container.RegisterTypeAs<BookingTestDataHelper, BookingTestDataHelper>();
    }
}

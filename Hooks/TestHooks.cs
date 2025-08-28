using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Reqnroll.BoDi;
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
        var services = new ServiceCollection();
        services.AddRestfulBookerServices(); // Registers BaseClient, BookingClient, LoggingHelper, ConfigManager
        services.AddScoped<BookingTestDataHelper>();

        var provider = services.BuildServiceProvider();

        // Register DI objects to SpecFlow container
        _container.RegisterInstanceAs(provider.GetRequiredService<ConfigManager>());
        _container.RegisterInstanceAs(provider.GetRequiredService<LoggingHelper>());
        _container.RegisterInstanceAs(provider.GetRequiredService<BookingClient>());
        _container.RegisterInstanceAs(provider.GetRequiredService<BaseClient>());
        _container.RegisterInstanceAs(provider.GetRequiredService<BookingTestDataHelper>());
        _container.RegisterInstanceAs(provider.GetRequiredService<ILogger<BookingSteps>>());
    }
}

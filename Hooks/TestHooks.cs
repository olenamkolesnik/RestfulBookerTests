using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Reqnroll.BoDi;
using RestfulBookerTests.Clients;
using RestfulBookerTests.Configuration;
using RestfulBookerTests.Extensions; // for GetRequiredServiceSafe
using RestfulBookerTests.Helpers;
using RestfulBookerTests.Utils;

[Binding]
public sealed class TestHooks
{
    private readonly IObjectContainer _container;
    private ServiceProvider? _provider;

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
        services.AddRestfulBookerServices();

        _provider = services.BuildServiceProvider();

        // Register DI objects to SpecFlow container using safe resolution
        _container.RegisterInstanceAs(_provider.GetRequiredServiceSafe<ConfigManager>("BeforeScenario - ConfigManager"));
        _container.RegisterInstanceAs(_provider.GetRequiredServiceSafe<LoggingHelper>("BeforeScenario - LoggingHelper"));

        // Clients
        _container.RegisterInstanceAs(_provider.GetRequiredServiceSafe<BookingClient>("BeforeScenario - BookingClient"));
        _container.RegisterInstanceAs(_provider.GetRequiredServiceSafe<BaseClient>("BeforeScenario - BaseClient"));

        // Logger for steps
        _container.RegisterInstanceAs(_provider.GetRequiredServiceSafe<ILogger<BookingSteps>>("BeforeScenario - ILogger<BookingSteps>"));
    }

    [AfterScenario]
    public void AfterScenario()
    {
        if (_provider != null)
        {
            // Dispose all scoped/transient services that implement IDisposable
            if (_provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _provider = null;
        }
    }
}
